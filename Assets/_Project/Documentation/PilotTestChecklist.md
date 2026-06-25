# Oystermouth Castle AR Pilot Test Checklist

## Device List

- Android phone, recent flagship, Android 14 or newer
- Android phone, mid-range, Android 12 or newer
- Android tablet, 10 inch class
- Low-memory Android device if available
- Optional iPhone/iPad comparison device for future iOS planning

Record for each device:

- Device model
- OS version
- Screen size
- Install type: APK or AAB internal test
- Build version
- Battery level at start/end
- Thermal warning or throttling

## Lighting Tests

- Bright outdoor daylight
- Cloudy daylight
- Late afternoon low light
- Interior chapel shade
- Visitor standing with back to strong light
- Camera pointed from the information board toward the chapel wall

Record:

- Time to localization
- Tracking stability
- Whether guidance text was needed
- Whether visitors could read UI outdoors

## Offline Tests

- Launch app with mobile data and Wi-Fi disabled
- Start chapel experience from default path
- Confirm Area Target dataset loads
- Confirm fresco and UI appear
- Confirm narration fallback behavior if audio is local
- Confirm analytics logs remain console-only and do not block use

## Tracking Tests

- Start near information board
- Start too close to chapel wall
- Start too far from chapel wall
- Move slowly across chapel space
- Rotate device away until tracking is lost
- Return camera to chapel wall
- Test 15 second guidance message
- Test 30 second guidance message

Record:

- First localization time
- Number of tracking losses
- Recovery time after tracking loss
- Any drift between reconstruction and chapel geometry

## Visitor Usability Tests

- Can visitor understand what to do from Welcome, Language, and Safety screens?
- Can visitor start the AR experience without staff help?
- Can visitor switch English/Welsh?
- Can visitor find Audio, Information, Before/After, and Exit?
- Does the Safety copy feel clear without being alarming?
- Does the Information panel feel readable outdoors?
- Does Before/After make sense without explanation?

## Staff Feedback Questions

- Is the chapel positioning guidance accurate for the site?
- Is the language appropriate for Oystermouth Castle visitors?
- Are Welsh and English labels acceptable?
- Is any heritage wording inaccurate or too speculative?
- Are there site safety issues caused by where visitors stand?
- Should the information board QR be larger, lower, or duplicated?

## Failure Cases To Record

- App does not open from QR
- Deep link opens app but wrong experience appears
- Camera permission denied
- Vuforia fails to initialize
- Area Target never localizes
- Tracking drifts badly after localization
- Narration does not play
- Audio is too quiet outdoors
- UI unreadable in sunlight
- App freezes or crashes
- Device overheats
- Battery drains unusually fast
