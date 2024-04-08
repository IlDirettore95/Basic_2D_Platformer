# Basic_2D_Platformer
This is the personal project for the 2nd level of DBGA Game Programming course Blended

I used a procedural content generation (WFC) to build three level using predefined chunks.
I used a data driven approach to define chuncks and constraints for the WFC algorithm. I built a tool with a GUI to insert several settings for the levels which then translates this data in an XML format. This file is then read at runtime.

The 2D character controller is done using a state machine (state pattern), ai agent like structure (sensors, decision making, actuators) using a state machine for the movement and an architecture like one described in AI for Games by Ian Millington. 
