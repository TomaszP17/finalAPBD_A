namespace FinalAPBD_A.DTOs;

public record AddBookDto(
    string Title,
    List<int> Genres
    );