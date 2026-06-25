# Oystermouth Castle AR UI Implementation Guide

## Design Intent

The UI must read as a public heritage interpretation system for Oystermouth Castle, Swansea Council, Cadw, schools, and public visitors. Bloodlines UI assets are used only as source material for frames, sliders, and controls after retheming. The final interface must avoid fantasy, horror, combat, rune, skull, or RPG visual language.

## Required Screens

1. Splash Screen
2. Welcome Screen
3. Language Screen
4. Safety Screen
5. AR Experience Screen
6. Information Screen
7. Credits Screen

## Visitor Flow

```text
Splash
  -> Welcome
  -> Language
  -> Safety
  -> AR Experience
       -> Searching for Chapel...
       -> Chapel Found
       -> Fresco Reconstruction
       -> Info / Audio / Before-After / Language / Exit
  -> Credits
```

## Prefab Hierarchy

```text
UI_AppRoot
  HeritageThemeApplier
  SafeArea
    ScreenLayer
      UI_SplashScreen
      UI_WelcomeScreen
      UI_LanguageScreen
      UI_SafetyScreen
      UI_CreditsScreen
    ARHudLayer
      UI_ARHud
        TopStatusToast
          LocalizationStatusPresenter
          StatusLabel
        BottomToolbar
          AudioButton
          LanguageButton
          InfoButton
          BeforeAfterButton
          ExitButton
        NarrationMiniPlayer
          NarrationPlayerUI
          PlayPauseButton
          ProgressSlider
          ReplayButton
    ModalLayer
      UI_InfoPanel
      UI_LanguageModal
```

## Theme Setup

Create a `HeritageUITheme` asset in:

```text
Assets/_Project/Data/Themes/HeritageUITheme.asset
```

Recommended palette:

```text
Medieval Stone      #6E6A5F
Warm Limestone      #B8AD93
Parchment           #F2E7C9
Aged Parchment      #D8C79D
Heritage Gold       #B58A3A
Deep Slate Text     #252A2C
Moss Accent         #5D6F4F
Muted Chapel Blue   #4D6F82
Warning             #8A4A3B
```

Attach `HeritageThemeApplier` to `UI_AppRoot`, assign the theme asset, then use:

- `HeritageThemedGraphic` on panels, button backgrounds, icons, slider fills, and overlays.
- `HeritageThemedText` on TextMeshPro labels.
- `LocalizationStatusPresenter` on the AR status toast.
- `NarrationPlayerUI` on the narration player root.

## Bloodlines Retheme Rules

- Replace red fills and red glows with heritage gold or warm limestone.
- Replace near-black panels with parchment or translucent stone.
- Remove or avoid rune-heavy, demonic, skull, blood, blade, or gothic motifs.
- Use bevels as carved stone, aged bronze, or conservation-case material.
- Prefer square or lightly rounded buttons over dramatic fantasy silhouettes.
- Keep all HUD controls quiet, readable, and small enough to preserve the chapel view.

## AR HUD

```text
Top center:
  Searching for Chapel...
  Chapel Found

Bottom:
  Audio
  Language
  Information
  Before / After
  Exit
```

The top localization toast fades out after a successful chapel localization. `LocalizationStatusPresenter` provides the English and Welsh status strings:

```text
Searching for Chapel...
Chapel Found
Keep the chapel in view
Return to the chapel start point
```

```text
Chwilio am y Capel...
Wedi canfod y Capel
Cadwch y capel yn y golwg
Dychwelwch at fan cychwyn y capel
```

## Narration Player

Use a rethemed Bloodlines horizontal slider for progress. Wire:

- Play/Pause button to `NarrationPlayerUI`.
- Replay button to `NarrationPlayerUI`.
- Slider to `NarrationPlayerUI`.
- `NarrationManager` to the same player.

The player supports play, pause, replay, seek, current time, duration, and completion.

## Information Panels

Use `InfoPanelContent` ScriptableObjects for historical panels. Each panel supports:

- English/Welsh title
- English/Welsh body text
- Images
- English/Welsh audio
- Research sources

Panel layout:

```text
Header
  Title
  Close
Media
  Image or carousel
Body
  Scrollable interpretive text
Audio
  NarrationPlayerUI
Footer
  Sources / credits
```

## Phone Layout

- Portrait-first.
- HUD buttons at bottom, one row.
- Minimum target size: 56 x 56.
- Info panels use 92% screen width.
- Narration player docks above bottom HUD.

## Tablet Layout

- Support portrait and landscape.
- HUD can remain bottom center or move to lower-right.
- Info panels can be 560-720 px wide.
- Use two-column image/text layouts when space permits.

## Accessibility Rules

- Body text minimum 18sp.
- Button labels minimum 18sp.
- Touch targets minimum 48 x 48, preferred 56 x 56.
- Use 4.5:1 contrast for body text.
- Do not place text directly over camera feed without a backing panel.
- Do not use color alone for tracking or playback states.
- Keep reduced-motion variants for transitions.
- Provide captions or transcripts for narration.
- Welsh and English must have equal status.

## Priority Build Order

1. Create `HeritageUITheme.asset`.
2. Retheme Bloodlines button, panel, slider, and toggle sprites.
3. Build `UI_AppRoot` and shared UI prefabs.
4. Build Splash, Welcome, Language, Safety, AR HUD, Info, and Credits screens.
5. Connect `LocalizationStatusPresenter` to `AreaTargetLocalizationManager`.
6. Connect `NarrationPlayerUI` to `NarrationManager`.
7. Connect before/after control to `BeforeAfterController`.
8. Populate `InfoPanelContent` and `NarrationTrack` assets for English and Welsh.
9. Test phone portrait, phone landscape recovery, tablet portrait, and tablet landscape.
10. Test outdoors on site for glare, tracking state clarity, and safe movement.
