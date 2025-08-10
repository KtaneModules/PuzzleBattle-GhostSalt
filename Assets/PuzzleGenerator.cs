using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleGenerator
{
    public Puzzle Puzzle;

    public PuzzleGenerator()
    {
        startOver:

        Suit[] allSuits = { Suit.Spade, Suit.Heart, Suit.Club, Suit.Diamond };
        var allPossibleArrangements =
        (from a in allSuits
         from b in allSuits.Where(s => s != a)
         from c in allSuits.Where(s => s != a && s != b)
         from d in allSuits.Where(s => s != a && s != b && s != c)
         select new[] { a, b, c, d }).ToList();

        var chosenArrangement = allPossibleArrangements.PickRandom();

        var allPossibleRules = new List<IRule>()
            {
                new HeartNextToClub(),
                new ClubNextToDiamond(),
                new DiamondNextToHeart(),

                new HeartNotNextToClub(),
                new ClubNotNextToDiamond(),
                new DiamondNotNextToHeart(),

                new HeartThenClub(),
                new ClubThenDiamond(),
                new DiamondThenHeart(),

                new HeartThenDiamond(),
                new ClubThenHeart(),
                new DiamondThenClub(),

                new SpadeNextToHeart(),
                new SpadeNextToClub(),
                new SpadeNextToDiamond(),

                new SpadeNotNextToHeart(),
                new SpadeNotNextToClub(),
                new SpadeNotNextToDiamond(),

                new EndHeart(),
                new EndClub(),
                new EndDiamond(),

                new NotEndHeart(),
                new NotEndClub(),
                new NotEndDiamond(),

            }.Where(x => x.Matches(chosenArrangement)).ToList().Shuffle();

        var chosenRules = new List<IRule>();
        var ruleIx = 0;

        while (!IsUniqueSolution(chosenArrangement, chosenRules, allPossibleArrangements))
        {
            chosenRules.Add(allPossibleRules[ruleIx]);
            ruleIx++;
        }

        for (int i = chosenRules.Count - 1; i >= 0; i--)
        {
            IRule ruleToTest = chosenRules[i];

            List<IRule> testRules = new List<IRule>();
            foreach (IRule r in chosenRules)
                if (r != ruleToTest)
                    testRules.Add(r);

            if (IsUniqueSolution(chosenArrangement, testRules, allPossibleArrangements))
                chosenRules.RemoveAt(i);
        }

        if (chosenRules.Count() > 4 || chosenRules.Count() < 3)
            goto startOver;

        if (chosenRules.Count() == 3)
            chosenRules.Add(new NoRule());

        chosenRules = chosenRules.Shuffle();
        Puzzle = new Puzzle(chosenArrangement, chosenRules);
    }

    public static bool IsUniqueSolution(Suit[] solution, List<IRule> rules, List<Suit[]> allArrangements)
    {
        var validArrangements = allArrangements.Where(arr => rules.All(rule => rule.Matches(arr))).ToList();

        if (validArrangements.Count != 1)
            return false;

        for (int i = 0; i < 4; i++)
            if (validArrangements[0][i] != solution[i])
                return false;
        return true;
    }
}

public class Puzzle
{
    public Suit[] Cards;
    public List<IRule> Rules;

    public Puzzle(Suit[] cards, List<IRule> rules)
    {
        Cards = cards;
        Rules = rules;
    }
}

public enum Suit { Spade, Heart, Club, Diamond };

public interface IRule
{
    int ID { get; }
    string Name { get; }
    bool Matches(Suit[] cards);
}

#region Rules A

public class HeartNextToClub : IRule
{
    public int ID { get { return 0; } }
    public string Name { get { return "A heart is next to a club."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Heart && cards[i + 1] == Suit.Club || cards[i] == Suit.Club && cards[i + 1] == Suit.Heart)
                return true;
        return false;
    }
}

public class ClubNextToDiamond : IRule
{
    public int ID { get { return 1; } }
    public string Name { get { return "A club is next to a diamond."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Club && cards[i + 1] == Suit.Diamond || cards[i] == Suit.Diamond && cards[i + 1] == Suit.Club)
                return true;
        return false;
    }
}

public class DiamondNextToHeart : IRule
{
    public int ID { get { return 2; } }
    public string Name { get { return "A diamond is next to a heart."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Diamond && cards[i + 1] == Suit.Heart || cards[i] == Suit.Heart && cards[i + 1] == Suit.Diamond)
                return true;
        return false;
    }
}

#endregion
#region Rules B

public class HeartNotNextToClub : IRule
{
    public int ID { get { return 3; } }
    public string Name { get { return "A heart is not next to a club."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Heart && cards[i + 1] == Suit.Club || cards[i] == Suit.Club && cards[i + 1] == Suit.Heart)
                return false;
        return true;
    }
}

public class ClubNotNextToDiamond : IRule
{
    public int ID { get { return 4; } }
    public string Name { get { return "A club is not next to a diamond."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Club && cards[i + 1] == Suit.Diamond || cards[i] == Suit.Diamond && cards[i + 1] == Suit.Club)
                return false;
        return true;
    }
}

public class DiamondNotNextToHeart : IRule
{
    public int ID { get { return 5; } }
    public string Name { get { return "A diamond is not next to a heart."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Diamond && cards[i + 1] == Suit.Heart || cards[i] == Suit.Heart && cards[i + 1] == Suit.Diamond)
                return false;
        return true;
    }
}

#endregion
#region Rules C

public class HeartThenClub : IRule
{
    public int ID { get { return 6; } }
    public string Name { get { return "A heart is directly to the left of a club."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Heart && cards[i + 1] == Suit.Club)
                return true;
        return false;
    }
}

public class ClubThenDiamond : IRule
{
    public int ID { get { return 7; } }
    public string Name { get { return "A club is directly to the left of a diamond."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Club && cards[i + 1] == Suit.Diamond)
                return true;
        return false;
    }
}

public class DiamondThenHeart : IRule
{
    public int ID { get { return 8; } }
    public string Name { get { return "A diamond is directly to the left of a heart."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Diamond && cards[i + 1] == Suit.Heart)
                return true;
        return false;
    }
}

public class HeartThenDiamond : IRule
{
    public int ID { get { return 9; } }
    public string Name { get { return "A heart is directly to the left of a diamond."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Heart && cards[i + 1] == Suit.Diamond)
                return true;
        return false;
    }
}

public class ClubThenHeart : IRule
{
    public int ID { get { return 10; } }
    public string Name { get { return "A club is directly to the left of a heart."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Club && cards[i + 1] == Suit.Heart)
                return true;
        return false;
    }
}

public class DiamondThenClub : IRule
{
    public int ID { get { return 11; } }
    public string Name { get { return "A diamond is directly to the left of a club."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Diamond && cards[i + 1] == Suit.Club)
                return true;
        return false;
    }
}

#endregion
#region Rules D

public class SpadeNextToHeart : IRule
{
    public int ID { get { return 12; } }
    public string Name { get { return "A spade is next to a heart."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Spade && cards[i + 1] == Suit.Heart || cards[i] == Suit.Heart && cards[i + 1] == Suit.Spade)
                return true;
        return false;
    }
}

public class SpadeNextToClub : IRule
{
    public int ID { get { return 13; } }
    public string Name { get { return "A spade is next to a club."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Spade && cards[i + 1] == Suit.Club || cards[i] == Suit.Club && cards[i + 1] == Suit.Spade)
                return true;
        return false;
    }
}

public class SpadeNextToDiamond : IRule
{
    public int ID { get { return 14; } }
    public string Name { get { return "A spade is next to a diamond."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Spade && cards[i + 1] == Suit.Diamond || cards[i] == Suit.Diamond && cards[i + 1] == Suit.Spade)
                return true;
        return false;
    }
}
#endregion
#region Rules E

public class SpadeNotNextToHeart : IRule
{
    public int ID { get { return 15; } }
    public string Name { get { return "A spade is not next to a heart."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Spade && cards[i + 1] == Suit.Heart || cards[i] == Suit.Heart && cards[i + 1] == Suit.Spade)
                return false;
        return true;
    }
}

public class SpadeNotNextToClub : IRule
{
    public int ID { get { return 16; } }
    public string Name { get { return "A spade is not next to a club."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Spade && cards[i + 1] == Suit.Club || cards[i] == Suit.Club && cards[i + 1] == Suit.Spade)
                return false;
        return true;
    }
}

public class SpadeNotNextToDiamond : IRule
{
    public int ID { get { return 17; } }
    public string Name { get { return "A spade is not next to a diamond."; } }

    public bool Matches(Suit[] cards)
    {
        for (int i = 0; i < cards.Count() - 1; i++)
            if (cards[i] == Suit.Spade && cards[i + 1] == Suit.Diamond || cards[i] == Suit.Diamond && cards[i + 1] == Suit.Spade)
                return false;
        return true;
    }
}

#endregion
#region Rules F

public class EndHeart : IRule
{
    public int ID { get { return 18; } }
    public string Name { get { return "One of the cards on the far left or far right is a heart."; } }

    public bool Matches(Suit[] cards)
    {
        return cards.First() == Suit.Heart || cards.Last() == Suit.Heart;
    }
}

public class EndClub : IRule
{
    public int ID { get { return 19; } }
    public string Name { get { return "One of the cards on the far left or far right is a club."; } }

    public bool Matches(Suit[] cards)
    {
        return cards.First() == Suit.Club || cards.Last() == Suit.Club;
    }
}

public class EndDiamond : IRule
{
    public int ID { get { return 20; } }
    public string Name { get { return "One of the cards on the far left or far right is a diamond."; } }

    public bool Matches(Suit[] cards)
    {
        return cards.First() == Suit.Diamond || cards.Last() == Suit.Diamond;
    }
}

#endregion
#region Rules G

public class NotEndHeart : IRule
{
    public int ID { get { return 21; } }
    public string Name { get { return "None of the cards on the far left or far right are hearts."; } }

    public bool Matches(Suit[] cards)
    {
        return cards.First() != Suit.Heart && cards.Last() != Suit.Heart;
    }
}

public class NotEndClub : IRule
{
    public int ID { get { return 22; } }
    public string Name { get { return "None of the cards on the far left or far right are clubs."; } }

    public bool Matches(Suit[] cards)
    {
        return cards.First() != Suit.Club && cards.Last() != Suit.Club;
    }
}

public class NotEndDiamond : IRule
{
    public int ID { get { return 23; } }
    public string Name { get { return "None of the cards on the far left or far right are diamonds."; } }

    public bool Matches(Suit[] cards)
    {
        return cards.First() != Suit.Diamond && cards.Last() != Suit.Diamond;
    }
}

#endregion

public class NoRule : IRule
{
    public int ID { get { return 999; } }
    public string Name { get { return "Always applies."; } }

    public bool Matches(Suit[] cards) { return true; }
}