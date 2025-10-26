/* * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *  Author: Timeless Puck (2025)                       *
 *  This code is open and free to use for any purpose. *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CoolCustomText.Source;

public class CustomText
{
    private readonly SpriteBatch _spriteBatch;
    private readonly float _lineHeight;

    private FxText[] _fxTexts;
    private string[][] _noFxTextsParts;
    private float _time;


    public SpriteFont Font { get; set; }

    public string Text { get; set; }

    public Color Color { get; set; }

    public Color ShadowColor { get; set; }

    public Vector2 ShadowOffset { get; set; }

    public Vector2 Dimension { get; set; }

    /// <summary>
    /// The origin of the text is (0,0), so the position refers to the top left position.
    /// </summary>
    public Vector2 Position { get; set; }

    public Vector2 Offset { get; set; }

    public Vector2 Padding { get; set; }

    /// <summary>
    /// The scale of the dimension, the font size and the padding aren't affected.
    /// </summary>
    /// <remarks>
    /// To change the font size, you need to edit your spritefont file.
    /// </remarks>
    public Vector2 Scale { get; set; }


    public CustomText(Game game, string fontName, string text, Vector2 position, Vector2 dimension, Vector2 offset = default,
        Vector2 padding = default, Vector2? scale = null, Color? color = null, Color? shadowColor = null, Vector2? shadowOffset = null)
    {
        Font = game.Content.Load<SpriteFont>(fontName);
        ShadowColor = shadowColor ?? Color.Transparent;
        ShadowOffset = shadowOffset ?? new(-4f, 4f);
        Color = color ?? Color.White;
        Dimension = dimension;
        Position = position;
        Padding = padding;
        Offset = offset;
        Text = text;
        Scale = scale ?? Vector2.One;

        _lineHeight = Font.MeasureString(" ").Y;
        _spriteBatch = game.Services.GetService<SpriteBatch>();
    }

    private Vector2 DrawFxText(FxText fxText, Vector2 nextCharPos)
    {
        float charWidth;
        int lineLength = 0;
        Vector2 nextFxCharPos = GetNextFxCharPosition(lineLength, nextCharPos, fxText);

        if (fxText.Shake) fxText.ResetRand();

        for (int i = 0; i < fxText.InnerText.Length; i++)
        {
            char c = fxText.InnerText[i];

            if (c == '\n')
            {
                nextCharPos = new(Position.X + Padding.X, nextCharPos.Y + _lineHeight);

                lineLength = 0;
                charWidth = 0f;
            }
            else
            {
                Color color = fxText.PaletteRotator?.NextColor ?? Color;
                Vector2 origin = Vector2.Zero;
                float rotation = 0f;

                if (fxText.Hang)
                {
                    origin = new(Font.MeasureString(c.ToString()).X / 2f, 0f);
                    rotation = MathHelper.ToRadians(MathF.Sin(_time * fxText.HangFrequency + i) * fxText.HangAmplitude);
                    nextFxCharPos = new(nextFxCharPos.X + origin.X, nextFxCharPos.Y);
                }

                Color shadowColor = color == Color ? ShadowColor : new(color.ToVector4() * 0.45f + Vector4.UnitW * 255f * color.A);
                DrawString(c.ToString(), nextFxCharPos, color, rotation, origin, shadowColor);

                lineLength++;
                charWidth = Font.MeasureString(c.ToString()).X;
            }

            nextCharPos = new(nextCharPos.X + charWidth, nextCharPos.Y);
            nextFxCharPos = GetNextFxCharPosition(lineLength, nextCharPos, fxText);
        }

        fxText.PaletteRotator?.RestartRotation();

        return nextCharPos;
    }

    private Vector2 GetNextFxCharPosition(int lineLength, Vector2 nextCharPos, FxText fxText)
    {
        return new Vector2()
        {
            X = nextCharPos.X +
                (fxText.Shake ? MathF.Sin(fxText.Rand.Next()) * fxText.ShakeStrength : 0f),

            Y = nextCharPos.Y +
                (fxText.Shake ? MathF.Sin(fxText.Rand.Next()) * fxText.ShakeStrength : 0f) +
                (fxText.Wave ? MathF.Sin(_time * fxText.WaveFrequency + lineLength) * fxText.WaveAmplitude : 0f)
        };
    }

    private List<string> SliceLongWord(string longWord)
    {
        List<string> words = [];
        StringBuilder word = new();

        foreach (char c in longWord)
        {
            word.Append(c);

            if (Font.MeasureString(word).X > Dimension.X * Scale.X - 2f * Padding.X)
            {
                word.Remove(word.Length - 1, 1);
                words.Add(word.ToString());
                word.Clear();
                word.Append(c);
            }
        }

        words.Add(word.ToString());

        return words;
    }

    private string ProcessFxTexts(string text)
    {
        Regex regex = new(@"<fx\s+([0-9,]+)>(.*?)</fx>", RegexOptions.Singleline);
        var matches = regex.Matches(text);
        int ignoredCharCount = 0;

        _fxTexts = new FxText[matches.Count];

        // Process all extracted fx texts.
        for (int i = 0; i < matches.Count; i++)
        {
            Match m = matches[i];

            string[] values = m.Groups[1].Value.Split(',');
            string innerText = m.Groups[2].Value;

            int startIdx = m.Index - ignoredCharCount;
            int colorProfil = 0, waveProfil = 0, shakeProfil = 0, hangProfil = 0;

            if (values.Length == 4)
            {
                _ = int.TryParse(values[0], out colorProfil);
                _ = int.TryParse(values[1], out waveProfil);
                _ = int.TryParse(values[2], out shakeProfil);
                _ = int.TryParse(values[3], out hangProfil);
            }

            _fxTexts[i] = new(startIdx, innerText.Length, colorProfil, waveProfil, shakeProfil, hangProfil);

            ignoredCharCount += m.Length - innerText.Length;
        }

        // Return the text without fx tags.
        return regex.Replace(text, m => m.Groups[2].Value);
    }

    private void AdjustFxTextsIndexes(List<int> addedChars)
    {
        if (addedChars.Count == 0) return;

        foreach (FxText fxText in _fxTexts)
        {
            foreach (int pos in addedChars)
            {
                // A char was added in the inner text so we update the fx text's length.
                if (pos >= fxText.StartIdx && pos <= fxText.EndIdx)
                    fxText.Length++;

                // A char was added before the start of the fx text: we increase the start index.
                else if (pos < fxText.StartIdx)
                    fxText.StartIdx++;
            }
        }
    }

    private void GenerateNoFxTextsParts(string output)
    {
        int startIdx = 0;

        _noFxTextsParts = new string[_fxTexts.Length + 1][];

        for (int i = 0; i < _fxTexts.Length; i++)
        {
            int length = _fxTexts[i].StartIdx - startIdx;
            string noFxPart = length > 0 ? output.Substring(startIdx, length) : string.Empty;

            _noFxTextsParts[i] = noFxPart.Split('\n');

            startIdx = _fxTexts[i].EndIdx + 1;
        }

        _noFxTextsParts[^1] = output.Substring(startIdx).Split('\n');
    }

    private string FilterUnsupportedChars(string text)
    {
        StringBuilder sb = new();

        foreach (char c in text)
            sb.Append((!Font.Characters.Contains(c) && (c != '\n')) ? '?' : c);

        return sb.ToString();
    }

    private void DrawString(string text, Vector2 position, Color color, float rotation = 0f, Vector2 origin = default, Color? shadowColor = null)
    {
        // Draw text shadow.
        if (ShadowColor != Color.Transparent)
            _spriteBatch.DrawString(Font, text, position + ShadowOffset, shadowColor ?? ShadowColor, rotation, origin, 1f, SpriteEffects.None, 0f);

        _spriteBatch.DrawString(Font, text, position, color, rotation, origin, 1f, SpriteEffects.None, 0f);
    }

    public void Draw()
    {
        Vector2 nextCharPos = Position + Padding;

        // Each no-fx text parts follow an fx text.
        for (int i = 0; i < _noFxTextsParts.Length; i++)
        {
            string[] noTextPart = _noFxTextsParts[i];

            float initialLineStartX = nextCharPos.X;

            // Draw no-fx text first line (may be empty)
            if (noTextPart[0] != string.Empty)
                DrawString(noTextPart[0], nextCharPos, Color);

            // Draw no-fx text subsequent lines (each starts at Position.X)
            for (int j = 1; j < noTextPart.Length; j++)
            {
                nextCharPos = new(Position.X + Padding.X, nextCharPos.Y + _lineHeight);

                if (noTextPart[j] != string.Empty)
                    DrawString(noTextPart[j], nextCharPos, Color);
            }

            // Determine the start X of the last drawn line:
            // if there's only one line, it started at initialLineStartX, otherwise, it started at Position.X.
            float lastLineStartX = (noTextPart.Length == 1) ? initialLineStartX : Position.X + Padding.X;

            if (noTextPart[^1] != string.Empty)
                nextCharPos = new(lastLineStartX + Font.MeasureString(noTextPart[^1]).X, nextCharPos.Y);

            // Then draw the fx text, if any.
            if (_fxTexts.Length > 0 && i < _fxTexts.Length)
                nextCharPos = DrawFxText(_fxTexts[i], nextCharPos);
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var fxText in _fxTexts)
            fxText.Update(deltaTime);

        _time += deltaTime;
    }

    public void Refresh()
    {
        string cleanedText = ProcessFxTexts(FilterUnsupportedChars(Text));
        string[] rawWords = cleanedText.Split(' ');
        StringBuilder line = new();
        StringBuilder output = new();
        List<int> addedChars = []; // Useful to adjust the indexes of fx texts set just before.
        List<int> longWordsPartsPos = [];
        List<string> words = [];

        // Slice words that are longer than a line.
        foreach (string word in rawWords)
        {
            float wordWidth = Font.MeasureString(word).X;

            if (wordWidth > Dimension.X * Scale.X - 2f * Padding.X)
            {
                List<string> longWordsParts = SliceLongWord(word);
                for (int i = 0; i < longWordsParts.Count; i++)
                {
                    string part = longWordsParts[i];

                    // Don't add the pos of the first part of the long word.
                    if (i != 0) longWordsPartsPos.Add(words.Count);

                    words.Add(part);
                }
            }
            else words.Add(word);
        }

        // Build output by placing the words within the set dimension.
        for (int i = 0; i < words.Count; i++)
        {
            string testLine = line.Length == 0 ? words[i] : line + " " + words[i];

            if (Font.MeasureString(testLine).X > Dimension.X * Scale.X - 2f * Padding.X)
            {
                if (i > 0) line.Append('\n');

                output.Append(line);

                // A new line was inserted between two parts of the same long word,
                // which increases the output length, unlike a regular new line that
                // simply replaces a space between two separate words.
                if (longWordsPartsPos.Contains(i))
                    addedChars.Add(output.Length - 1);

                line.Clear();

                // A word can be empty if it's from consecutives spaces, in this case we add a space to the output.
                if (words[i] == string.Empty)
                {
                    line.Append(' ');
                    addedChars.Add(output.Length);
                }
                else line.Append(words[i]);
            }
            else
            {
                // Add a space between words unless the current word start the line.
                if (line.Length > 0) line.Append(' ');

                line.Append(words[i]);
            }
        }

        if (line.Length > 0) output.Append(line);

        string strOutput = output.ToString();

        // Adjust the indexes of fx texts based on the added chars that increase the output's length.
        AdjustFxTextsIndexes(addedChars);

        // Generate the inner text of fx texts according to their indexes.
        foreach (FxText fxText in _fxTexts)
            fxText.InnerText = strOutput.Substring(fxText.StartIdx, fxText.Length);

        GenerateNoFxTextsParts(strOutput);
    }

    /// <summary>
    /// Nested class that defines a configuration on an fx text.
    /// </summary>
    /// <remarks>
    /// This class is nested because it's only useful for a custom text.
    /// </remarks>
    private class FxText
    {
        /// <summary>
        /// The different color palette profiles. Add as many as you want by following the syntax below.
        /// </summary>
        private readonly static Dictionary<int, Tuple<ColorPalette, float>> ColorProfiles = new()
        {
            // Color Palette, Rotation Speed 
            [1] = new(ColorPalette.Rainbow, 0.075f),
            [2] = new(ColorPalette.Elemental, 0.075f),
            [3] = new(ColorPalette.SoftCandy, 0.075f),
            [4] = new(ColorPalette.SoftPurple, 0.075f),
            [5] = new(ColorPalette.Retro, 0.075f),
            [6] = new(ColorPalette.White, 0.075f),
            [7] = new(ColorPalette.TenMovingRed, 0.125f)
        };

        /// <summary>
        /// The different wave profiles. Add as many as you want by following the syntax below.
        /// </summary>
        private readonly static Dictionary<int, Tuple<float, float>> WaveProfils = new()
        {
            // Wave Frequency, Wave Amplitude
            [1] = new(8f, 8f)
        };

        /// <summary>
        /// The different shake profiles. Add as many as you want by following the syntax below.
        /// </summary>
        public static Dictionary<int, Tuple<float, float>> ShakeProfils = new()
        {
            // Shake Interval, Shake Strength
            [1] = new(0.06f, 3f),
        };

        /// <summary>
        /// The different hang profiles. Add as many as you want by following the syntax below.
        /// </summary>
        public static Dictionary<int, Tuple<float, float>> HangProfils = new()
        {
            // Hang Frequency, Hang Amplitude
            [1] = new(6f, 12f)
        };

        private float _shakeTime;
        private int _randSeed;


        public int Length { get; set; }

        public int StartIdx { get; set; }

        public string InnerText { get; set; }

        public PaletteRotator PaletteRotator { get; }

        public bool Wave { get; }

        public float WaveFrequency { get; }

        public float WaveAmplitude { get; }

        public bool Hang { get; }

        public float HangFrequency { get; }

        public float HangAmplitude { get; }

        public bool Shake { get; }

        public float ShakeInterval { get; }

        public float ShakeStrength { get; }

        public Random Rand { get; private set; }

        public int EndIdx => StartIdx + Length - 1;


        public FxText(int startIdx, int length, int colorProfil, int waveProfil, int shakeProfil, int hangProfil)
        {
            Length = length;
            StartIdx = startIdx;

            if (ColorProfiles.TryGetValue(colorProfil, out Tuple<ColorPalette, float> colorValues))
                PaletteRotator = new(colorValues.Item1, colorValues.Item2);

            if (WaveProfils.TryGetValue(waveProfil, out Tuple<float, float> waveValues))
            {
                (WaveFrequency, WaveAmplitude) = waveValues;
                Wave = true;
            }

            if (ShakeProfils.TryGetValue(shakeProfil, out Tuple<float, float> shakeValues))
            {
                (ShakeInterval, ShakeStrength) = shakeValues;
                _randSeed = (int)DateTime.Now.Ticks;
                ResetRand();
                Shake = true;
            }

            if (HangProfils.TryGetValue(hangProfil, out Tuple<float, float> hangValues))
            {
                (HangFrequency, HangAmplitude) = hangValues;
                Hang = true;
            }
        }

        public void Update(float deltaTime)
        {
            PaletteRotator?.Update(deltaTime);

            if (Shake)
            {
                _shakeTime += deltaTime;

                if (_shakeTime >= ShakeInterval)
                {
                    _shakeTime = 0f;
                    _randSeed = (int)DateTime.Now.Ticks;
                }
            }
        }

        public void ResetRand() => Rand = new Random(_randSeed);
    };
}
