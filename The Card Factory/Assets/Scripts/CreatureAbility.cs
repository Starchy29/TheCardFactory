using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AbilityTrigger {
    public string Text { get; private set; }
    public CostModifier CalculateCost { get; private set; }

    public delegate int CostModifier(int baseCost);

    public AbilityTrigger(string text, CostModifier costCalculator) {
        Text = text;
        CalculateCost = costCalculator;
    }
}

public struct AbilityEffect {
    public string Text { get; private set; }
    public int Cost { get; private set; }
    public Colors ColorRequirement { get; private set; }

    public AbilityEffect(string text, int cost, Colors colorRequirement) {
        Text = text;
        Cost = cost;
        ColorRequirement = colorRequirement;
    }
}

public class CreatureAbility
{
    public string Text { get { return Trigger.Text + Effect.Text + "."; } }
    public int Cost { get { return Trigger.CalculateCost(Effect.Cost); } }
    public bool Active { get; set; }

    public AbilityTrigger Trigger;
    public AbilityEffect Effect;

    public CreatureAbility() {
        Trigger = Triggers[0];
        Effect = Effects[0];
        Active = false;
    }

    public static AbilityTrigger[] Triggers = new AbilityTrigger[5] {
        new AbilityTrigger("When CARDNAME enters the battlefield, ", (cost) => { return cost + 2; }),
        new AbilityTrigger("When CARDNAME dies, ", (cost) => { return cost; }),
        new AbilityTrigger("Whenever CARDNAME deals combat damage to a player, ", (cost) => { return cost * 2 - 1; }),
        new AbilityTrigger("(>): ", (cost) => { return cost * 2; }),
        new AbilityTrigger("2, (>): ", (cost) => { return cost; })
    };

    public static AbilityEffect[] Effects = new AbilityEffect[6] {
        new AbilityEffect("draw a card", 3, new Colors(false, true, false, false, true)),
        new AbilityEffect("surveil 2", 1, new Colors(false, true, true, false, true)),
        new AbilityEffect("create a treasure token", 3, new Colors(false, false, false, true, true)),
        new AbilityEffect("create a 1/1 colorless Human creature token", 2, new Colors(true, false, false, true, true)),
        new AbilityEffect("put a +1/+1 counter on target creature you control", 2, new Colors(true, false, true, false, true)),
        new AbilityEffect("you gain 3 life", 1, new Colors(true, false, true, false, true))
    };
}
