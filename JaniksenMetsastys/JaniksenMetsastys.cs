using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

public class JaniksenMetsastys : PhysicsGame
{
    public const double LIIKUTUSVOIMA = 250;
    public const double HYPPYVOIMA = 900;
    public const int KENTAN_KOKO = 45;


    public override void Begin()
    {
        SetWindowSize(1920, 1080);
        CenterWindow();

        TileMap kentta = TileMap.FromLevelAsset("kentta");

        Gravity = new Vector(0, -1000);

        LuoKentta();
        LisaaNappaimet();




        Vector missaHugo = new Vector(0, 0);
        LuoHugo(missaHugo, 80.0, 80.0);
    }


    public void LuoHugo(Vector paikka, double korkeus, double leveys)
    {
        PlatformCharacter hugo = new PlatformCharacter(korkeus, leveys);
        hugo.Position = paikka;
        hugo.Tag = "hugo";
        AddCollisionHandler(hugo, "pupujussi", HugoTormasiPupujussiin);
        hugo.Image = LoadImage("hugo");
        Add(hugo);

        Camera.Follow(hugo);
        Camera.ZoomFactor = 2;

        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta Hugoa oikealle", hugo, LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikuta Hugoa vasemmalle", hugo, -LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", hugo, HYPPYVOIMA);
    }


    public void LuoPupujussi(Vector paikka, double korkeus, double leveys)
    {
        Image[] janikset = new[] { LoadImage("pupujussi"), LoadImage("rusakko") };
        PlatformCharacter pupujussi = new PlatformCharacter(korkeus, leveys);
        pupujussi.Position = paikka;
        pupujussi.Tag = "pupujussi";

        pupujussi.Image = LoadImage("pupujussi");
        Add(pupujussi);
    }


    public void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta.txt");
        kentta.SetTileMethod('X', LuoEste);
        kentta.SetTileMethod('p', LuoPupujussi);
        kentta.Execute(KENTAN_KOKO, KENTAN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.DarkForestGreen, Color.White);
        //Level.Background.Image = LoadImage("metsa");
    }


    public void HugoTormasiPupujussiin(PhysicsObject tormaaja, PhysicsObject kohdeJohonTormataan)
    {
        MessageDisplay.Add("Sait kiinni pupujussin!");
        kohdeJohonTormataan.Destroy();
    }


    public void LuoEste(Vector kohta, double leveys, double korkeus)
    {
        PhysicsObject este = new PhysicsObject(leveys, korkeus);
        este.Position = kohta;
        este.Color = Color.Brown;
        este.MakeStatic();
        Add(este);
    }


    public void Liikuta(PlatformCharacter liikutettava, double suunta)
    {
        liikutettava.Walk(suunta);

    }


    public void Hyppaa(PlatformCharacter hyppaavaOlio, double voima)
    {
        hyppaavaOlio.Jump(voima);
    }


    public void LisaaNappaimet()
    {


        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
}