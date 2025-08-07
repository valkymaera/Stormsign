# Stormsign
A simple if not ugly feature timer companion app for Dune: Awakening focused on tracking sandstorms, with QoL timers.
It's 768 x 768, not currently resizeable, and other platforms are planned but lower priority (WebGL, Linux, _maybe_ mobile)

<img width="768" height="768" alt="image" src="https://github.com/user-attachments/assets/bfd404e7-b8cd-4744-acce-4619e11f564e" />


The primary feature is to **track and warn you** of the 15 minute window in which a sandstorm might appear.
this window always begins 45 minutes after the previous storm, meaning a new storm will appear 45 to 60 minutes after the previous appears.

It has additional 'bonus' features of a settable game clock with dew and day cycle alerts, and a couple chest timers that report 45 minutes after they're clicked.


# First Run

This app requires that you know when the last storm was in order for it to be useful for storms (you can still use the timers for whatever of course). 
It tracks the time since the last storm you sighted, and gives you a risk window with audio alerts, as you approach and enter the next storm window.
When you launch the app, it won't know when the most recent storm was, so it will give you a risk window of 0 to 60 minutes.

When you sight a storm in-game, hold the 'storm sighted' button and this will start the timer properly toward the next storm, 
giving you the minutes remaining until the start and end of the storm window.


# UI Breakdown

<img width="774" height="769" alt="image" src="https://github.com/user-attachments/assets/b0ca8893-49e6-41c8-9e96-8234ea162303" />

The central buttons are the original purpose of the timer, before extra features were added.

## Storm Sighted
Hold for 1 second when you see a storm in-game.
This resets the timer in the app so you'll see the minutes until the next storm.
If you saw the storm a little after it spawned, you'll be able to adjust the timer to estimate that.

## Cancel Sighting
This throws away the most recent storm data you sighted. Might be useful if you saw a worm breach on the compass and thought it was a storm, for example. 
Not that such a thing has happened to me, of course. 

Once discarded, the next most recent storm gets used instead, showing its direction if you logged it, and updating the timer based on that storm's sighting time.

## Clear Timer
This resets the timer and pauses it, and that's it. It'll screw up your window tracking but it's weird to have a timer without the option to control it so I put it in.
I don't recommend using it, and might end up removing it.

---

<img width="768" height="763" alt="image" src="https://github.com/user-attachments/assets/c40fbcad-68c8-4c5d-9fa9-e3b8b30a96fe" />

## Timer
The timer will tick _up_ so you can see how long it's been since the last storm was sighted.
You can pause it or play it by clicking either of those buttons, the pair is a single toggle. 
I don't recommend using them, but again it's a timer so it seemed weird to omit.

## Offset Arrows
Above and below the timer are arrows you can click to add or subtract 30 seconds from it per click.
This lets you adjust for storms that you caught late, for example, by adding time to the timer with the up arrow.
I use these often if I catch a storm part way into the map already, as they seem to take around 5 minutes to cross the basin.

---
# RISK
The concept of 'risk' in this app relates to how risky it is to go out on an expidition where cover is not guaranteed, such as an overland buggy run in an open area,
particularly near the edge of the map where there may be limited warning. It also accounts for a wiggle room of innacurately sighting storms and still having a feel for 
how imminent a storm is.

<img width="775" height="773" alt="image" src="https://github.com/user-attachments/assets/73dc0bba-b57f-4139-94a0-c4d78f18d9d4" />


## Risk Meter
At the top is a colorful meter that displays the timer as it represents risk brackets for expeditions. 
As the timer nears 60 minutes, the indicator on the meter will move to the right until it is at the end of the meter.

### Low Risk (green, leftward)
The first half hour is considered low risk. There's a high chance of getting where you need to be and back before seeing one. 
In the low risk segment, you have a _minimum_ of 30 minutes before a storm appears.

### Moderate Risk (yellow, center-left)
Moderate risk lasts for ten minutes. It starts at the 30m marker and ends at 40m.
In the moderate risk segment, you have under 30 minutes before a sandstorm appears on the map, possibly as little as 15 minutes.

### High Risk (red, center-right)
High risk marks the end of the 'safe' segment of the meter. It starts at 40m and only lasts 5 minutes before entering extreme.
These are the last 5 minutes where a sandstorm will definitely not spawn. Possibly less if your sighting wasn't timed well.
The absolute maximum time you have is 20 minutes.

### Extreme Risk (violet and up, far right)
In the extreme risk segment, which starts at 45 minutes, a sandstorm can appear at any time.
You are in the spawn window. You have 0 to 15 minutes and should seek shelter if sandstorms are a threat to you.
Avoid the edge of the map, where they may arrive with limited warning. Not that such a thing has happened to me, naturally.


<img width="771" height="769" alt="image" src="https://github.com/user-attachments/assets/4e85e84e-fa9a-4a77-9e3e-ca19a88fc2ff" />


## Risk Banner
on the bottom of the screen is a colorized risk banner, matching the risk color if colors help you.
This banner has two sets of numbers visible

### Storm Window
The larger numbers on top. One of the main features, this shows the minimum and maximum time until the next storm.
It is a fifteen minute window, based on the 'extreme' threshold that starts 45 minutes after the previous storm.
These two numbers were the driving force behind me creating the app so I didn't have to do math in my head.

### Risk Bracket
The small bottom numbers. You can ignore these. They represent the thresholds for each risk level.
I thought they would be useful but I don't think they have been and they're probably not well communicated.
For posterity, they represented what 'risk bracket' threshold you were in (low, medium, high, extreme) represented by time window instead of label.
Since implementing, I have never cared about it, and unless someone really likes them they'll probably be removed later.


<img width="768" height="764" alt="image" src="https://github.com/user-attachments/assets/1f8b8d68-453b-4762-8c0e-1048133256f4" />

## Direction Log
Optionally, you can log the direction of the sandstorm after it's sighted. It saves it to file along with storm time data, for optional analysis.
I added this to see if there is a pattern to the directions, but it might have some value in visualizing safe / unsafe areas. Maybe.
The 'clock' values on the map correspond to the world map entrance points.

### Creating a storm vector
To create a directional arrow, left click anywhere on the map to set the start location, and right click to set the end (arrowhead) location.
You can change them at any time, or drag them around. If autosave is active, then the updated vector is written to file as soon as you're done.

If you cancel a sighting, any vector you created for the previous storm will be loaded instead, and that previous storm will be what you overwrite.

IMPORTANT: this direction affects the storm you have already sighted. If you put the direction in before sighting it, you are modifying a previous storm.
This is why you aren't able to create a direction before you sight your first storm... there's nothing to direct.

### Lock Input
If you click the lock icon in the top left, the map will no longer receive clicks. This can help avoid accidentally messing up last storm's vector,
half an hour into a new expedition when you've forgotten what it was so you can't fix it. Not that such a thing happened to me, mind you.

### Zoom
opens a full screen variant of the map, that shares the same vector values. Just a bit easier to see.

### Data Quality Slider
This only saves its value to file, it has no mechanical effect. It's a way for me to log how accurate I feel my estimations on direction and timing were,
so I can exclude storm data I'm not confident in when I build an analysis app that reads it. Nothing more.
If you don't plan on using the data, feel free to ignore it, or associate its value with something more meaningful to you.

<img width="772" height="768" alt="image" src="https://github.com/user-attachments/assets/3ea971b2-6b08-41dc-8b3c-ecbe4d5eafa3" />

## Game Clock
Tracks the day/night cycle primarily for the purpose of alerting dew times, but can also alert sunrise and sunset if desired.
It helps to avoid being ready for dew 10 minutes before it appears and then forgetting about it until the sun comes up. Not that such a thing has happened to me nightly.

To sync the clock with the game, click it. a large version will appear that you can dial into the right time.
Once you set the clock, it saves the set time and rotation, so next time you launch the app it should still be pretty accurate.

## Dew Indicator
Before sunrise, the night has 3 minutes of decent dew time (it reads as 'poor' in the game but is still good), followed by 4.5 minutes of optimal dew time.
(The bright blue slice before sunrise did not seem to be better than the 3 minutes preceeding it)

A time label next to the dew indicator shows how many minutes before dew begins. Once started, it remains at zero minutes until sunrise.

When in 'decent' dew time, there will be an audio alert "Dew has begun", and the dewdrop indicator will be half full.
When in optimal dew time, the audio will say "dew optimal", and the indicator will be full.

If sunrise / nightfall audio is enabled, it will notify you 30 seconds before sunrise / nightfall.

<img width="773" height="769" alt="image" src="https://github.com/user-attachments/assets/16323844-70b0-4e9d-b80a-a923f06e98be" />
## Chest Timers
You can click these timers to set them to 45 minutes. When they get to zero they will notify you.


---

# General UI Elements
On the top and bottom of the screen you'll see save and audio options.

## File Saving
If Autosave is true, then storm data will be written to file every time it changes, such as from a storm sighting or map vector change.
If autosave is false, you'll have to click the manual save button if and when you want to save. It should turn yellow when there is something new.
Files are saved to the app's streaming assets folder, in JSON format

## Audio
You can adjust the main audio slider, mute the main audio, and open the settings to mute individual alerts.
By default, the alerts for low risk, sunrise, and nightfall are muted.




















