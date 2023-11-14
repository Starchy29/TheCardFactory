using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Colors {
    public bool white;
    public bool blue;
    public bool black;
    public bool red;
    public bool green;

    public Colors(bool white, bool blue, bool black, bool red, bool green) {
        this.white = white;
        this.blue = blue;
        this.black = black;
        this.red = red;
        this.green = green;
    }

    public int Total() {
        return (white ? 1 : 0) + (blue ? 1 : 0) + (black ? 1 : 0) + (red ? 1 : 0) + (green ? 1 : 0);
    }
}

public struct Ability {
    public int pointCost;
    public string text;
    public Colors colorRequirement;
}

public class CardMaker : MonoBehaviour
{
    private const int POINTS_PER_MANA = 3;

    public int PointTotal { get; private set; }
    public int PowerLevel { get { return PointTotal / POINTS_PER_MANA + 1; } }

    public Colors ColorIdentity { get; private set; }
    public int ManaValue { get; private set; }
    public int Power { get; private set; }
    public int Toughness { get; private set; }

    void Start() {
        
    }

    private void CalculatePoints() {
        PointTotal = ManaValue * POINTS_PER_MANA;
    }

    private void UpdateDisplay() {

    }

    private bool HasRequiredColor(Colors requirements) {
        return ColorIdentity.white && requirements.white ||
            ColorIdentity.blue && requirements.blue ||
            ColorIdentity.black && requirements.black ||
            ColorIdentity.red && requirements.red ||
            ColorIdentity.green && requirements.green;
    }
}
