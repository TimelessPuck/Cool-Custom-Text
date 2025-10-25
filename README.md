# Cool Custom Text (MonoGame)

![Visual example](Docs/CoolAnimatedText.gif "Visual exemple")

A small project that shows a cool way to use SpriteFont in MonoGame.

---

## Syntax and Application

To apply an effect to a specific part of the text, we use XML-like tag called 'fx'.  
In the visual exemple above the input text looks like this :  
```csharp
string text = "Hello stranger, are you <fx 2,0,0,1>good</fx> ?\n<fx 1,1,0,0>いいいいいいいいいいいいいい</fx><fx 6,0,1,0>This line is scared</fx> <fx 6,1,0,0>></fx><fx 7,0,0,0>0123456789</fx><fx 6,1,0,0><</fx>";
```

As you can see, one fx tag contains 4 numbers that define a profile for the effect:  
<fx Color Palette, Wave, Shake, Hang>  
Effects can be combine or can be ignored with 0.  
Custom texts support newlines and consecutives spaces.

Here, how to create a custom text:  
```csharp
string text = "This my <fx 2,0,0,1>cool custom text</fx> !";
Vector2 textDim = new(284f, 60f);
Vector2 position = new(50f, 50f);

_customText = new CustomText(this, "PixellariFont", text, position, textDim) // 'this' is Game1
{
    Scale = new(4f), // Scale the dimension. This is useful if you're working with scaled UI and want to have a coherent dimension.
    Color = new(255, 244, 196),
    Padding = new(20f, 0f),
    ShadowColor = new(128, 85, 111), // Color.Transparent to disable it.
};
_customText.Refresh(); // Don't forget to refresh the text after the initialization and after you change the text properties.
```

Don't forget to update and draw your custom text:
```
protected override void Update(GameTime gameTime)
{
    ...
    float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
    _customText.Update(deltaTime);
    ...    
}

protected override void Draw(GameTime gameTime)
{
    ...
    _customText.Draw();
    ...
}
```

---

## Add new profiles

Profiles are stored in static readonly dictionnaries in the nested class `FxText` in the class `CustomText`.  

To add a new profile for a specific effect, just follow the syntax.  

Here, what it looks like:

```csharp
// Palette color profiles
private readonly static Dictionary<int, Tuple<ColorPalette, float>> ColorProfiles = new()
{
    // Color Palette, Rotation Speed 
    [1] = new(ColorPalette.Rainbow, 0.075f),
    // New profile ! [2] = ...
}

// Wave profiles
private readonly static Dictionary<int, Tuple<float, float>> WaveProfils = new()
{
    // Wave Frequency, Wave Amplitude
    [1] = new(8f, 8f),
    // New profile ! [2] = ...
};

// Shake profiles
public static Dictionary<int, Tuple<float, float>> ShakeProfils = new()
{
    // Shake Interval, Shake Strength
    [1] = new(0.06f, 3f),
    // New profile ! [2] = ...
};

// Hang profiles
public static Dictionary<int, Tuple<float, float>> HangProfils = new()
{
    // Hang Frequency, Hang Amplitude
    [1] = new(6f, 12f),
    // New profile ! [2] = ....
};
```

The same applied to a new color palette.  
Profiles are stored in the class `PaletteRotator`, don't forget to add the name of the new color palette in the enum `ColorPalette`.

---

## License

Free to use for any purpose. 
