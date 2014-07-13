AudioPlayer.setup("../audio-code/player.swf",{ width: 425 }); 
AudioPlayer.embed("myaudio0" ,{soundFile: "../audio-code/js/audio0.mp3",
titles: "Introduction & Chp-1",
/*artists: "Darwaish Family",*/
autostart: "yes",
remaining: "yes",
rtl: "yes",
buffer : "10",
initialvolume : 10,
	
} ); 



AudioPlayer.embed("myaudio1" ,{soundFile: "../audio-code/audio1.mp3",
titles: "Chp-2 The Cow ",
artists: "Darwaish Family",
autostart: "no",
remaining: "yes",
rtl: "yes",
buffer : "10",
initialvolume : 10,
	
} ); 
