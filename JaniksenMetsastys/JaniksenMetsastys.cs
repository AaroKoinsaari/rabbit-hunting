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


    /// <summary>
    /// Luo pelaajan ohjattavan Hugo-koiran.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="korkeus"></param>
    /// <param name="leveys"></param>
    public void LuoHugo(Vector paikka, double korkeus, double leveys)
    {
        PlatformCharacter hugo = new PlatformCharacter(korkeus, leveys);
        hugo.Position = paikka;
        hugo.Tag = "hugo";
        AddCollisionHandler(hugo, "pupujussi", HugoTormasiPupujussiin);
        hugo.Image = LoadImage("hugo");

        hugo.Weapon = new AssaultRifle(30, 10);
        hugo.Weapon.ProjectileCollision = AmmusOsui;
        hugo.Weapon.AmmoIgnoresExplosions = false;
        hugo.Weapon.X = 10.0;
        hugo.Weapon.Y = -5.0;

        Add(hugo);

        Camera.Follow(hugo);
        Camera.ZoomFactor = 2;

        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liiku oikealle", hugo, LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liiku vasemmalle", hugo, -LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", hugo, HYPPYVOIMA);

        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "Ammu aseella", hugo);
        //Keyboard.Listen(Key.Space, ButtonState.Pressed, HeitaKranaatti, "Heitä kranaatti", hugo);
    }


    /// <summary>
    /// Ammutaan AssaultRifle-aseella.
    /// </summary>
    /// <param name="pelaaja"></param>
    public void AmmuAseella(PlatformCharacter pelaaja)
    {
        PhysicsObject panos = pelaaja.Weapon.Shoot();
    }


    /// <summary>
    /// Törmäyskäsittelijä kun ammus osuu olioon.
    /// </summary>
    /// <param name="ammus"></param>
    /// <param name="kohde"></param>
    public void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        kohde.Destroy();
        ammus.Destroy();
    }


    ///// <summary>
    ///// Tehdään pelaajalle heitettävä kranaatti.
    ///// </summary>
    ///// <param name="pelaaja"></param>
    //public void HeitaKranaatti(PlatformCharacter pelaaja)
    //{
    //    Grenade kranaatti = new Grenade(4.0);
    //    pelaaja.Throw(kranaatti, Angle.FromDegrees(30), 10000);
    //    kranaatti.Explosion.ShockwaveReachesObject += KranaattiOsui;
    //}


    public void KranaattiOsui(IPhysicsObject rajahdyksenKohde, Vector v)
    {
        rajahdyksenKohde.Destroy();
    }


    /// <summary>
    /// Luo jäniksen tai rusakon.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="korkeus"></param>
    /// <param name="leveys"></param>
    public void LuoPupujussi(Vector paikka, double korkeus, double leveys)
    {
        Image[] kuvat = LoadImages("pupujussi", "rusakko", "ilves");
        PlatformCharacter pupujussi = new PlatformCharacter(korkeus, leveys);
        pupujussi.Position = paikka;
        pupujussi.Tag = "pupujussi";

        for (int i = 0; i < kuvat.Length; i++)
        {
            int alkio = RandomGen.NextInt(kuvat.Length);
            pupujussi.Image = kuvat[alkio];
        }

        Add(pupujussi);
    }


    /// <summary>
    /// Luodaan pelikenttä.
    /// </summary>
    public void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta.txt");
        kentta.SetTileMethod('X', LuoEste);
        kentta.SetTileMethod('p', LuoPupujussi);
        kentta.Execute(KENTAN_KOKO, KENTAN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.DarkForestGreen, Color.White);
    }


    /// <summary>
    /// Törmäyskäsittelijä, kun pelaaja osuu jänikseen.
    /// </summary>
    /// <param name="tormaaja"></param>
    /// <param name="kohdeJohonTormataan"></param>
    public void HugoTormasiPupujussiin(PhysicsObject tormaaja, PhysicsObject kohdeJohonTormataan)
    {
        //MessageDisplay.Add("Sait kiinni pupujussin!");
        kohdeJohonTormataan.Destroy();
    }


    /// <summary>
    /// Kentän esteet/tasot, joille hypitään.
    /// </summary>
    /// <param name="kohta"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    public void LuoEste(Vector kohta, double leveys, double korkeus)
    {
        PhysicsObject este = new PhysicsObject(leveys, korkeus);
        este.Position = kohta;
        este.Color = Color.Brown;
        este.MakeStatic();
        Add(este);
    }


    /// <summary>
    /// Hugon liikuttaminen.
    /// </summary>
    /// <param name="liikutettava"></param>
    /// <param name="suunta"></param>
    public void Liikuta(PlatformCharacter liikutettava, double suunta)
    {
        liikutettava.Walk(suunta);
    }


    /// <summary>
    /// Pelaaja hyppää.
    /// </summary>
    /// <param name="hyppaavaOlio"></param>
    /// <param name="voima">Millä voimalla hyppää.</param>
    public void Hyppaa(PlatformCharacter hyppaavaOlio, double voima)
    {
        hyppaavaOlio.Jump(voima);
    }


    /// <summary>
    /// Lisätään pelin näppäimet.
    /// </summary>
    public void LisaaNappaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
}