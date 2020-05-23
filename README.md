# Transfer Photos from Card
📷 This is a console-app I made because I take a lot of photos (and sometimes video too) and manually moving files off my camera’s memory-card can get tedious.

🚀 It’s very specific to my workflow, and probably won’t be something you want if you’re not me, but you can [download the executable here](https://github.com/DuncanRitchie/TransferPhotosFromCard/blob/master/TransferPhotosFromCard/bin/Release/TransferPhotosFromCard.exe).

🔌 It waits for a device to be inserted in drive `D:\` , then lists the files, grouped according to the action it expects me (the user) to take on them.

🖼️ If there are `.jpg` files on the disk, I get the option to move them to my Desktop.

📹 If there are `.mts` files (video clips), I get the option to move them to my Desktop and rename them to their date created (my camera saves them with a simple serial `0000.mts`, `0001.mts`, ..., which I dislike).

🗃️ If there are files that my camera uses to manage its internal catalogue, I get the option to delete them; I also get the option to replace them with an empty catalogue so my camera doesn’t tell me photos are missing. The files for the empty catalogue comprise the `DefaultDiskContents` folder.

🗄️ If there are other files, I get the options to move, move and rename, or ignore them.

📂 Finally the program deletes any empty folders on the card.

💻 Files are moved to my Desktop because I use Lightroom to edit my photos and videos, and its import process will organize them into folders on my hard drive, but it won’t delete files from a memory-card. So I need to move my files off the card before I import them to Lightroom.

👨‍💻 Made by [Duncan Ritchie](https://www.duncanritchie.co.uk/) using .NET Framework (C#) in Visual Studio, 2020 May 23.
