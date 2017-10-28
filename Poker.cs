using System;
using System.Collections.Generic; // Used for the various Lists
using System.Linq; // Used for the OrderBy and GroupBy queries

namespace PokerExerciseCSharp
{
    class Poker
    {
        // Card faces, in ascending order of value
        public enum CardFace { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
        
        // Card suits, in alpha order
        public enum CardSuit { Club, Diamond, Heart, Spade }
        
        // Game stages
        public enum GameStage { Deal, Draw, Score, End }
        
        // Hand ranks, in ascending order of rank
        public enum HandRank { Unranked, Nothing, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush }

        public class Card // Definition for a card
        {
            // Holds card face
            public CardFace face;
            
            // Holds card suit
            public CardSuit suit;

            // Class constructor that takes a card face and suit
            public Card(CardFace cardFace, CardSuit cardSuit)
            {
                face = cardFace;
                suit = cardSuit;
            }
        }

        public class CardDeck // Definition for a deck of cards (52 card deck - no jokers)
        {
            // Holds collection of cards
            public List<Card> cards = new List<Card>();

            // Constructor - creates the initial deck
            public CardDeck()
            {
                // Deck is created in order of suit, then face value
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                    foreach (CardFace face in Enum.GetValues(typeof(CardFace)))
                    {
                        Card newCard = new Card(face, suit);
                        cards.Add(newCard);
                    }
            }

            // Shuffles the deck a single time
            public void Shuffle()
            {
                Random random = new Random();
                List<Card> shuffledCards = new List<Card>();
                // Loop until the new shuffled cards collection has 52 cards
                while (shuffledCards.Count < 52)
                {
                    // Get random card number from 0-51 (cards collection is zero-based)
                    int randomCardNum = random.Next(0, 52);
                    Card randomCard = cards[randomCardNum];
                    // Only add this randomly chosen card if it is not already in the shuffled deck
                    if (!shuffledCards.Contains(randomCard))
                    {
                        shuffledCards.Add(randomCard);
                    }
                }
                // Replace original cards with these shuffled cards
                cards = shuffledCards;
            }

            // Shuffles the deck a specified number of times
            public void Shuffle(int numTimes)
            {
                for (int i=1; i<=numTimes; i++)
                    Shuffle(); // Call single shuffle
            }

            // Deal single card from top of deck to specified hand
            public Hand DealCard(Hand hand)
            {
                // Cards are dealt from the top of the deck
                Card dealtCard = cards[0];
                hand.cards.Add(dealtCard);
                // Remove top card from deck since it is now in player hand
                cards.RemoveAt(0);
                return hand;
            }

            // Draw single card from top of deck and replace specified card (number) in specified hand
            public Hand DrawCard(Hand hand, int drawCard)
            {
                // Cards are dealt from the top of the deck, original card is discarded
                Card dealtCard = cards[0];
                // Discard original card in hand (zero-based)
                hand.cards.RemoveAt(drawCard-1);
                // Insert new drawn card in hand at the same location
                hand.cards.Insert(drawCard-1, dealtCard);
                // Remove top card from deck since it is now in player hand
                cards.RemoveAt(0);
                return hand;
            }
        }

        public class Hand // Definition for a card hand
        {
            // Holds collection of cards
            public List<Card> cards = new List<Card>();

            // Holds hand rank - starts with being unranked
            public HandRank rank = HandRank.Unranked;

            // Holds face value of high card in rank
            public CardFace highCard;

            // Holds the player number associated with this hand
            public int playerNumber;

            // Returns multiple lines as a string with the card number and face value plus suit
            public string DisplayHand()
            {
                string returnHand = "";
                // Loop through collection of cards in hand (zero-based)
                for (int cardCount=0; cardCount<cards.Count; cardCount++)
                {
                    returnHand += "Card " + (cardCount+1).ToString() + ": " + cards[cardCount].face.ToString() + " of " + cards[cardCount].suit.ToString() + "s\n";
                }
                return returnHand;
            }

            // Private method to check rank - "Pair"
            private void CheckPair()
            {
                // Group cards by face value
                var tmpCards = cards.GroupBy(Card => Card.face);
                foreach (var faceGroup in tmpCards)
                {
                    if (faceGroup.Count()>=2) // Look for a count of 2 per face
                    {
                        rank = HandRank.Pair;
                        highCard = faceGroup.Key; // Face value of pair
                    }
                }
            }

            // Private method to check rank - "Two Pair"
            private void CheckTwoPair()
            {
                // Group cards by face value
                var tmpCards = cards.GroupBy(Card => Card.face);
                // Holds the number of pairs that are found
                int numPairs = 0;
                // Holds the highest card found with a pair
                CardFace tempHigh = CardFace.Two;
                foreach (var faceGroup in tmpCards)
                {
                    if (faceGroup.Count()>=2) // Look for a count of 2 per face
                    {
                        numPairs++;
                        if (faceGroup.Key >= tempHigh)
                            tempHigh = faceGroup.Key; // Set new high card
                    }
                }
                if (numPairs>=2) // Only set rank if we find 2 pairs
                {
                    rank = HandRank.TwoPair;
                    highCard = tempHigh;
                }
            }

            // Private method to check rank - "Three of a Kind"
            private void CheckThreeOfAKind()
            {
                // Group cards by face value
                var tmpCards = cards.GroupBy(Card => Card.face);
                foreach (var faceGroup in tmpCards)
                {
                    if (faceGroup.Count()>=3) // Look for a count of 3 per face
                    {
                        rank = HandRank.ThreeOfAKind;
                        highCard = faceGroup.Key;
                    }
                }
            }

            // Private method to check rank - "Straight"
            private void CheckStraight()
            {
                // Holds the total difference in values between cards (cards are already sorted by value)
                int[] diffCount = new int[4];
                // Calculate difference in card values between cards
                for (int numCard = 2; numCard <= 5; numCard++)
                {
                    diffCount[numCard - 2] = cards[numCard - 1].face - cards[numCard - 2].face;
                }
                if (diffCount[0] == 1 && diffCount[1] == 1 && diffCount[2] == 1 && diffCount[3] ==1)
                {
                    rank = HandRank.Straight; // Each card is 1 away from the next card
                }
                if (cards[0].face == CardFace.Two && cards[1].face == CardFace.Three && cards[2].face == CardFace.Four && cards[3].face == CardFace.Five && cards[4].face == CardFace.Ace)
                {
                    rank = HandRank.Straight; // Special case for Ace at the start of a straight
                }
                if (rank == HandRank.Straight)
                {
                    highCard = cards[4].face; // Set high card
                }
            }

            // Private method to check rank - "Flush"
            private void CheckFlush()
            {
                // Group cards by suit value
                var tmpCards = cards.GroupBy(Card => Card.suit);
                foreach (var suitGroup in tmpCards)
                {
                    if (suitGroup.Count()>=5) // Look for a single grouping of all 5 cards
                    {
                        rank = HandRank.Flush;
                    }
                }
                if (rank == HandRank.Flush)
                    highCard = cards[4].face; // Set high card
            }

            // Private method to check rank - "Full House"
            private void CheckFullHouse()
            {
                // Group cards by face value
                var tmpCards = cards.GroupBy(Card => Card.face);
                // Holds the number of two pair
                int numTwoPair = 0;
                // Holds the number of three pair
                int numThreePair = 0;
                // Holds the highest card found with a pair
                CardFace tempHigh = CardFace.Two;
                foreach (var faceGroup in tmpCards)
                {
                    if (faceGroup.Count()==2) // Look for a two pair
                    {
                        numTwoPair++;
                    }
                    if (faceGroup.Count()==3) // Look for a three pair (use the three pair for high card)
                    {
                        numThreePair++;
                        if (faceGroup.Key >= tempHigh)
                            tempHigh = faceGroup.Key;
                    }
                }
                if (numTwoPair==1 && numThreePair==1) // Only set rank if we find a 2 and a 3 pair
                {
                    rank = HandRank.FullHouse;
                    highCard = tempHigh;
                }
            }

            // Private method to check rank - "Four of a Kind"
            private void CheckFourOfAKind()
            {
                // Group cards by face value
                var tmpCards = cards.GroupBy(Card => Card.face);
                foreach (var faceGroup in tmpCards)
                {
                    if (faceGroup.Count()>=4) // Look for a count of 4 per face
                    {
                        rank = HandRank.FourOfAKind;
                        highCard = faceGroup.Key;
                    }
                }
            }

            // Private method to check rank - "Straight Flush" or "Royal Flush"
            private void CheckStraightFlush()
            {
                // First check to see if this is a flush at all
                CheckFlush();
                if (rank == HandRank.Flush) // If at least flush, check for straight or royal
                {
                    // Holds the total difference in values between cards (cards are already sorted by value)
                    int[] diffCount = new int[4];
                    // Calculate difference in card values between cards
                    for (int numCard = 2; numCard <= 5; numCard++)
                    {
                        diffCount[numCard - 2] = cards[numCard - 1].face - cards[numCard - 2].face;
                    }
                    if (cards[0].face == CardFace.Two && cards[1].face == CardFace.Three && cards[2].face == CardFace.Four && cards[3].face == CardFace.Five && cards[4].face == CardFace.Ace)
                    {
                        rank = HandRank.StraightFlush; // Special case for Ace at the start of a straight
                    }
                    if ((diffCount[0] == 1 && diffCount[1] == 1 && diffCount[2] == 1 && diffCount[3] ==1) && cards[4].face != CardFace.Ace)
                    {
                        rank = HandRank.StraightFlush; // Standard straight flush
                    }
                    if ((diffCount[0] == 1 && diffCount[1] == 1 && diffCount[2] == 1 && diffCount[3] ==1) && cards[4].face == CardFace.Ace)
                    {
                        rank = HandRank.RoyalFlush; // Ace at the end of a straight makes it a royal flush
                    }
                    if (rank == HandRank.StraightFlush || rank == HandRank.RoyalFlush) {
                        highCard = cards[4].face; // Set high card
                    }
                }
            }

            // Private method to sort cards based on face value
            private void SortCards()
            {
                cards.Sort((x, y) => x.face.CompareTo(y.face));
            }

            // Calculates the rank of the hand
            // First cards are sorted by face value, then each rank is "checked" in descending
            // order of rank (and it making a rank will keep it from any further checks)
            public void GetRank()
            {
                // Sort cards by face value
                SortCards();

                if (rank == HandRank.Unranked)
                    CheckStraightFlush();

                if (rank == HandRank.Unranked)
                    CheckFourOfAKind();

                if (rank == HandRank.Unranked)
                    CheckFullHouse();

                if (rank == HandRank.Unranked)
                    CheckFlush();

                if (rank == HandRank.Unranked)
                    CheckStraight();

                if (rank == HandRank.Unranked)
                    CheckThreeOfAKind();

                if (rank == HandRank.Unranked)
                    CheckTwoPair();

                if (rank == HandRank.Unranked)
                    CheckPair();

                if (rank == HandRank.Unranked) // If no rank is valud, then rank is "Nothing" and we just store the high card
                {
                    rank = HandRank.Nothing;
                    highCard = cards[4].face; // Set high card
                }

            }
        }

        // Returns the number of players based on input
        static int GetPlayers()
        {
            // Holds the input as a string
            string getInput;
            // Holds the input coverted to an int
            int inputResult;
            // Used by loop to continue to prompt until proper input
            bool valid = false;
            do
            {
                Console.Write("Please input number of players (2-7): ");
                // Get user input
                getInput = Console.ReadLine();
                Console.WriteLine("");
                if (int.TryParse(getInput, out inputResult)) // Check to see if input can be parsed as an int
                {
                    if (inputResult>=2 && inputResult<=7) // Only allow an int between 2 and 7
                    {
                        valid = true;
                    }
                }
            }
            while (!valid);
            return inputResult;
        }

        static bool GetComputerPlayer()
        {
            // Holds the input as a string
            string getInput;
            Console.Write("Computer is player? ");
            // Get user input
            getInput = Console.ReadLine();
            return (getInput.ToUpper().Trim() == "YES" || getInput.ToUpper().Trim() == "Y");
        }

        // Returns a collection of ints that represent the cards chosen from players
        // hand for the draw
        static List<int> GetDrawCards()
        {
            // Holds input as a string
            string getInput;
            // Holds collection of ints to be returned (start empty)
            List<int> returnDraw = new List<int>();
            // Get input
            getInput = Console.ReadLine();
            // Convert input to array of strings, split using ',' character
            string[] inputItems = getInput.Split(',');
            // Loop through array results
            for (int numItem=0; numItem<inputItems.Length; numItem++)
            {
                // Holds int value of element in the array
                int itemVal;
                if (int.TryParse(inputItems[numItem].Trim(), out itemVal)) // Check to see if string can be parsed as an int
                {
                    if (returnDraw.Count<5) // Only allow a maximum of 5 cards in draw - others are ignored
                        returnDraw.Add(itemVal);
                }
            }
            return returnDraw;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Poker!\n");

            // Get number of players
            int numPlayers = GetPlayers();

            // Get computer as player or not
            bool computerPlayer = GetComputerPlayer();

            // Create deck and shuffle it (3 times)
            CardDeck deck = new CardDeck();
            deck.Shuffle(3);

            // Tracks stage of game - Deal, Draw, Score and End
            GameStage gameStage = GameStage.Deal;

            // Create a collection of hands to represent each player
            List<Hand> hands = new List<Hand>();
            for (int playerHands=1; playerHands<=numPlayers; playerHands++)
            {
                Hand newHand = new Hand();
                newHand.playerNumber = playerHands; // Holds the player number - used at scoring since these will be sorted
                hands.Add(newHand);
            }

            // Main game loop
            do
            {
                switch (gameStage)
                {
                    case GameStage.Deal: // Deal cards to players
                        // Card loop - 5 cards
                        for (int numCards=1; numCards<=5; numCards++)
                        {
                            // Player loop
                            for (int numPlayer=1; numPlayer<=numPlayers; numPlayer++)
                            {
                                // Hands collection is zero-based
                                hands[numPlayer-1] = deck.DealCard(hands[numPlayer-1]);
                            }
                        }
                        Console.WriteLine("\nAll hands are dealt\n");
                        // Display hands for each player
                        for (int numPlayer=1; numPlayer<=numPlayers; numPlayer++)
                        {
                            Console.WriteLine("Player " + numPlayer.ToString() + " hand:");
                            Console.WriteLine(hands[numPlayer-1].DisplayHand());
                        }
                        // Change game stage
                        gameStage = GameStage.Draw;
                        break;
                    case GameStage.Draw: // Allow each player to draw cards
                        Console.WriteLine("\nNow time to choose draw\n");
                        // Redisplay hand for each player, then get input on which cards are part of the
                        // draw - input is card numbers seperated by commas
                        for (int numPlayer=1; numPlayer<=numPlayers; numPlayer++)
                        {
                            Console.WriteLine("Player " + numPlayer.ToString() + " hand:");
                            Console.WriteLine(hands[numPlayer-1].DisplayHand());
                            Console.WriteLine("\nEnter the cards you would like to use in the draw");
                            Console.Write("(card numbers seperated by commas, hit enter for none): ");
                            // Gets a collection of ints representing the card numbers in the draw (zero-based)
                            List<int> drawCards = GetDrawCards();
                            for (int drawCard=0; drawCard<drawCards.Count; drawCard++)
                            {
                                // Only do draw if the input card is a valid card number (1-5)
                                if (drawCards[drawCard]>=1 && drawCards[drawCard]<=5)
                                    hands[numPlayer-1] = deck.DrawCard(hands[numPlayer-1], drawCards[drawCard]);
                            }
                            Console.WriteLine("");
                        }
                        // Change game stage
                        gameStage = GameStage.Score;
                        break;
                    case GameStage.Score: // Calculate score by ranking each hand for players
                        Console.WriteLine("\nFinal hands of players\n");
                        // Redisplay hand for each player so that they see the results of the previous draw
                        // and can see both the rank of the hand and what the "high" card was for that rank
                        for (int numPlayer=1; numPlayer<=numPlayers; numPlayer++)
                        {
                            hands[numPlayer-1].GetRank();
                            Console.WriteLine("Player " + numPlayer.ToString() + " hand:");
                            Console.WriteLine(hands[numPlayer-1].DisplayHand());
                            Console.WriteLine("High Card: " + hands[numPlayer-1].highCard.ToString());
                            Console.WriteLine("RANK: " + hands[numPlayer-1].rank.ToString() + "\n");
                        }
                        // Order hands based on descending rank, then desending high card, then player number
                        // This means there are no ties - a tie is broken by the player number (should it be?)
                        List<Hand> orderedHands = hands.OrderByDescending(x => x.rank).ThenByDescending(x => x.highCard).ThenBy(x => x.playerNumber).ToList();
                        // Display winner player and that player's hand rank and high card
                        Console.WriteLine("Winner is player " + orderedHands[0].playerNumber.ToString());
                        Console.WriteLine("with a rank of: " + orderedHands[0].rank.ToString() + ", high card: " + orderedHands[0].highCard.ToString());
                        // Change game stage
                        gameStage = GameStage.End;
                        break;
                }
            }
            while (gameStage != GameStage.End);

            // End of game
            Console.WriteLine("\nThanks for playing!");

        }
    }
}
