/*
 * @author Lukáš Lízal 2018
 */
 
// represents stages of elastic pan process
// Start - finger on screen
// End - figer off screen (inertia)
// Done - inertia stops -> camera is still
public enum GestureState { Start, End, Done };
// represents different types of guiding between two nodes
// None - no guiding at all
// OneDirection - being guided only in direction towards destination node
// BothDirections - guided in both direction
public enum GuideType {BothDirections, OneDirection, None };
// represents main direction of pan
// Horizontal - camera moves on x axis
// Vertical - camera moves on y axis
public enum PanOrientation { Horizontal, Vertical };
// represents orientation inside any main direction axis
// Minus - camera moves in minus direction (in activated oriantation)
// Plus - camera moves to Plus direction
public enum PanDirection { Minus, Plus };
// represents direction in context of crossnodes direction
// HorizontalPlus ←
// HorizontalPlus →
// VerticallPlus ↑
// VerticallMinus ↓
public enum OrientationDirection {HorizontalPlus, HorizontalMinus, VerticalPlus, VerticalMinus}
// represents different kinds of areas of a corridor map
// FreezoneOut  - corridor part between two nodes;
//              - player can't change main direction of pan here;
//              - player moves freely along main direction without constraints
//              - inertia is applied
// FreezoneIn   - this area is inside of an crossroad node
//              - player can change between main directions of pan here;
//              - player moves freely in this space without constraints
//              - inertia is applied
// Bumpzone     - area inside of a crossroad node
//              - represents area behind the end of a list
//              - player can change between main directions of pan here
//              - player movment is constrained here by slowing down the speed and swinging player back to the border of Freezone
public enum NodeState { FreeZoneOut, FreeZoneIn, BumpZone, AttractionZone }
// represents axis
public enum Axis { X,Y,Z }

