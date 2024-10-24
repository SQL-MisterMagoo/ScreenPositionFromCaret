# Sample showing a way to get screen position from caret position in VS

This accounts for viewport scaling and scrolling, device DPI and responds to most changes.

## Known issues

Floating a window doesn't seem to fire any events that trigger the screen position to re-calculate

The test window in this sample is crude - just to show the calculation is working.