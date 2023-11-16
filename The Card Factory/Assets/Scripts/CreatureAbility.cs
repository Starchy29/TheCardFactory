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

public class CreatureAbility
{
    public string Text { get { return Trigger.Text + Effect.text + "."; } }
    public int Cost { get { return Trigger.CalculateCost(Effect.pointCost); } }
    public bool Active { get; set; }

    public AbilityTrigger Trigger;
    public Ability Effect;

    public CreatureAbility() {
        Trigger = Triggers[0];
        Effect = Effects[0];
        Active = false;
    }

    public static AbilityTrigger[] Triggers = new AbilityTrigger[7] {
        new AbilityTrigger("When CARDNAME enters the battlefield, ", (cost) => { return cost + 2; }),
        new AbilityTrigger("When CARDNAME dies, ", (cost) => { return cost; }),
        new AbilityTrigger("Sacrifice CARDNAME: ", (cost) => { return cost + 1; }),
        new AbilityTrigger("Whenever CARDNAME deals combat damage to a player, ", (cost) => { return cost * 2; }),
        new AbilityTrigger("(>): ", (cost) => { return cost * 2 + 2; }),
        new AbilityTrigger("(2), (>): ", (cost) => { return Mathf.CeilToInt(cost * 1.5f); }),
        new AbilityTrigger("(4): ", (cost) => { return cost + 2; }),
    };

    public static Ability[] Effects = new Ability[13] {
        new Ability("draw a card, then discard a card", 1, new Colors(false, true, false, true, false)),
        new Ability("you gain 3 life", 1, new Colors(true, false, true, false, true)),
        new Ability("CARDNAME deals 1 damage to each opponent", 2, new Colors(false, false, true, true, false)),
        new Ability("put a +1/+1 counter on target creature you control", 2, new Colors(true, false, true, false, true)),
        new Ability("draw a card", 5, new Colors(false, true, false, false, true)),
        new Ability("create a treasure token", 4, new Colors(false, false, false, true, true)),
        new Ability("create a 2/2 colorless Shapeshifter creature token", 5, new Colors(true, false, false, true, true)),
        new Ability("return target card from your graveyard to your hand", 6, new Colors(false, false, false, false, true)),
        new Ability("return target creature card from your graveyard to the battlefield", 12, new Colors(true, false, true, false, false)),
        new Ability("return target nonland permanent to its owner's hand", 6, new Colors(false, true, false, false, false)),
        new Ability("exile target nonland permanent until CARDNAME leaves the battlefield", 9, new Colors(true, false, false, false, false)),
        new Ability("destroy target artifact or enchantment", 6, new Colors(true, false, false, false, true)),
        new Ability("CARDNAME fights target creature an opponent controls", 4, new Colors(false, false, false, true, true))
    };
}
