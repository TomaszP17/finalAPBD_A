using System.Data;
using System.Data.SqlClient;
using FinalAPBD_A.DTOs;

namespace FinalAPBD_A.Services;

public interface IDbService
{
    Task<GetBookDto?> GetBookById(int id);
    Task<int> AddBookAsync(AddBookDto bookDto);
}

public class DbService(IConfiguration configuration) : IDbService
{
    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }


    public async Task<GetBookDto?> GetBookById(int id)
    {
        await using var connection = await GetConnection();
        var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = """
                                SELECT b.PK, b.title, g.NAME
                                FROM BOOKS b
                                LEFT JOIN BOOKS_GENRES bg ON b.PK = bg.FK_BOOK
                                LEFT JOIN GENRES g ON bg.FK_GENRE = g.pk
                                WHERE @id = b.PK;
                              """;

        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
        {
            return null;
        }

        GetBookDto bookDto = null;
        var genres = new List<string>();

        while (await reader.ReadAsync())
        {
            if (bookDto == null)
            {
                bookDto = new GetBookDto(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    new List<string>()
                );
            }

            if (!reader.IsDBNull(2))
            {
                genres.Add(reader.GetString(2));
            }
        }

        bookDto = bookDto with { Genres = genres };

        return bookDto;
    }

    public async Task<int> AddBookAsync(AddBookDto bookDto)
    {
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var bookCommand = new SqlCommand("INSERT INTO Books (title) " +
                                             "VALUES (@Title); SELECT SCOPE_IDENTITY();");
            bookCommand.Connection = connection;
            bookCommand.Transaction = (SqlTransaction)transaction;
            bookCommand.Parameters.AddWithValue("@Title", bookDto.Title);

            var bookId = (decimal)await bookCommand.ExecuteScalarAsync();

            foreach (var genreId in bookDto.Genres)
            {
                var genreBookCommand = new SqlCommand("INSERT INTO BOOKS_GENRES (FK_BOOK, FK_GENRE)" +
                                                      " VALUES (@Book_PK, @GENRE_PK);");
                genreBookCommand.Connection = connection;
                genreBookCommand.Transaction = (SqlTransaction)transaction;
                genreBookCommand.Parameters.AddWithValue("@Book_PK", bookId);
                genreBookCommand.Parameters.AddWithValue("@GENRE_PK", genreId);

                await genreBookCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return (int)bookId;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return -1;
        }
    }
}