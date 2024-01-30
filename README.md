# trivia-surubi
This is a fork from CorrientesCurioso created by @Matte4k aimed to improve the game since it'll be no longer maintained by its creator.
I've changed the game logic to use a plain .txt file to load the questions and answers.
The file is located in Data/StreamingAssets/questions.txt.
The sintaxis for each line is:

Category;Question?;Ans1;Ans2;Ans3;Ans4;CorrectAnsIndex

Each field separated by a point and comma and the CorrectAnsIndex goes from 0 to 3

I've also created a .prefab to .txt parser to parse all the current 83 questions at the moment and extract the questions and answers from each prefab located in Assets folder.
Sure it was worth it the time and it was very satisfying to not write by hand each question again.

Now everyone can add their own questions and answers to the game, give it a try!
New Features:
- Questions categories! Now you can filter which categories you want to play or go crazy and play them all
- Random answers order
- Load questions from .txt
- Game winning on all questions answered
- Capability to config a score limit
- Game winning on reached score limit