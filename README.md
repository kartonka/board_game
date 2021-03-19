# A board game template
This is a simple .Net WinForms board game template for a max. 4 person 30 fields discovery game. The players start from field one and can be moved by the mouse freely.

To play the game 4 things are needed in addition to the game's exe:
- A .csv file that contains the game title, the field titles, the field stories (multiple stories can be defined for a field). See an example in the _example configs_ folder.
- A folder with the field pictures. The pictures should be named the following way:
The picture that belongs to the 4th field's 2nd story should be called: "4_2.png". Remark: Other picture extensions (.bmp, .jpg, .gif, etc.) are supported as well.
- A picture that is shown as a cover picture for the undiscovered fields.
- A paths.ini file next to the .exe with the paths to the recently listed items. See an example in the _example config_ folder!!
