namespace GoogleVision;

public record class OcrResult(
    IReadOnlyList<TextBlock> TextBlocks
);