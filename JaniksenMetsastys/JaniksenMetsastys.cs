using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

/// @author Aaro Koinsaari
/// @version 2.4.2022
/// 
/// TODO: funktio, ks. https://tim.jyu.fi/answers/kurssit/tie/ohj1/2022k/demot/demo9?answerNumber=3&task=D9T5&user=aarkasko
/// TODO: taulukko/lista, ks. https://tim.jyu.fi/answers/kurssit/tie/ohj1/2022k/demot/demo9?answerNumber=5&task=tulostaSummat&user=aarkasko
/// TODO: silmukka, ks. https://tim.jyu.fi/answers/kurssit/tie/ohj1/2022k/demot/demo11?answerNumber=13&task=taulukot&user=aarkasko

/// <summary>
/// Pahisten ominaisuuksia.
/// </summary>
public class Pahis : PlatformCharacter
{
    private int elamiaJaljella;

    public Pahis(double leveys, double korkeus, int elamapisteet) : base(leveys, korkeus)
    {
        this.elamiaJaljella = elamapisteet;    
    }

    /// <summary>
    /// Osumankäsittelijä pahikselle.
    /// </summary>
    public void Osuma()
    {
        elamiaJaljella--;
        if (elamiaJaljella < 0)
        {
            this.Destroy();
        }
    }
}


/// <summary>
/// Jäniksen metsästys -peli.
/// </summary>
public class JaniksenMetsastys : PhysicsGame
{
    /// <summary>
    /// Pelaajan liikutusvoima sivuille.
    /// </summary>
    private const double LIIKUTUSVOIMA = 250;

    /// <summary>
    /// Pelaajan hyppyvoima.
    /// </summary>
    private const double HYPPYVOIMA = 900;

    /// <summary>
    /// Pelikentän koko.
    /// </summary>
    private const int KENTAN_KOKO = 45;

    /// <summary>
    /// Pistelaskuri.
    /// </summary>
    private IntMeter pistelaskuri;

    /// <summary>
    /// Pelin alustus.
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(1920, 1080);
        CenterWindow();

        Gravity = new Vector(0, -1000);

        LuoKentta();
        LisaaNappaimet();
        LuoPistelaskuri();
    }


    /// <summary>
    /// Luo pelaajan ohjattavan Hugo-koiran.
    /// </summary>
    /// <param name="paikka">Pelaajan kohta kentällä pelin alussa.</param>
    /// <param name="korkeus">Pelaajan korkeus.</param>
    /// <param name="leveys">Pelaajan leveys.</param>
    public void LuoPelaaja(Vector paikka, double korkeus, double leveys)
    {
        PlatformCharacter pelaaja = new PlatformCharacter(korkeus, leveys);
        pelaaja.Position = paikka;
        pelaaja.Tag = "pelaaja";
        pelaaja.Image = LoadImage("hugo");

        AddCollisionHandler(pelaaja, "pupujussi", OsuttiinPupujussiin);

        pelaaja.Weapon = new AssaultRifle(30, 10);
        pelaaja.Weapon.IsVisible = false;
        pelaaja.Weapon.AmmoIgnoresExplosions = false;
        pelaaja.Weapon.ProjectileCollision = AmmusOsui;

        Add(pelaaja);

        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 2;

        Keyboard.Listen(Key.Right, ButtonState.Down, delegate { pelaaja.Walk(LIIKUTUSVOIMA); }, "Liiku oikealle");
        Keyboard.Listen(Key.Left, ButtonState.Down, delegate { pelaaja.Walk(-LIIKUTUSVOIMA); }, "Liiku vasemmalle");
        Keyboard.Listen(Key.Up, ButtonState.Pressed, delegate { pelaaja.Jump(HYPPYVOIMA); }, "Hyppää");
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "Ammu aseella", pelaaja);
    }


    /// <summary>
    /// Ammutaan AssaultRifle-aseella.
    /// </summary>
    /// <param name="ampuja">Aseella ampuva olio.</param>
    public void AmmuAseella(PlatformCharacter ampuja)
    {
        ampuja.Weapon.Angle = ampuja.FacingDirection.Angle;
        PhysicsObject panos = ampuja.Weapon.Shoot();
        if (panos != null)
        {
            AddCollisionHandler<PhysicsObject, Pahis>(panos, "pahis", AmmusOsuiPahikseen);
            AddCollisionHandler<PhysicsObject, PlatformCharacter>(panos, "pupujussi", OsuttiinPupujussiin);
        }
    }


    /// <summary>
    /// Törmäyskäsittelijä kun ammus osuu olioon.
    /// </summary>
    /// <param name="ammus">Aseesta lähtevä ammus.</param>
    /// <param name="kohde">Kohde, johon ammus osuu.</param>
    public void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        ammus.Destroy();
    }


    /// <summary>
    /// Törmäyskäsittelijä, kun ammus osuu pahikseen.
    /// </summary>
    /// <param name="ammus">Aseesta lähtevä ammus.</param>
    /// <param name="pahis">Pahis, johon ammus osuuu.</param>
    public void AmmusOsuiPahikseen(PhysicsObject ammus, Pahis pahis)
    {
        ammus.Destroy();
        pahis.Osuma();
    }


    /// <summary>
    /// Törmäyskäsittelijä, kun osutaan pupujussiin.
    /// </summary>
    /// <param name="tormaaja">Törmäävä olio.</param>
    /// <param name="kohdeJohonTormataan">Olio, johon törmätään.</param>
    public void OsuttiinPupujussiin(PhysicsObject tormaaja, PhysicsObject kohdeJohonTormataan)
    {
        MessageDisplay.Add("Sait kiinni pupujussin!");
        pistelaskuri.Value++;
        kohdeJohonTormataan.Destroy();
    }


    /// <summary>
    /// Luo pelaajan metsästettävän pupujussin.
    /// </summary>
    /// <param name="paikka">Pupujussin paikka pelikentällä.</param>
    /// <param name="korkeus">Pupujussin korkeus.</param>
    /// <param name="leveys">Pupujussin leveys.</param>
    public void LuoPupujussi(Vector paikka, double korkeus, double leveys)
    {
        Image[] kuvat = LoadImages("jänis", "rusakko");
        PlatformCharacter pupujussi = new PlatformCharacter(korkeus, leveys);
        pupujussi.Position = paikka;
        pupujussi.Tag = "pupujussi";
        pupujussi.IgnoresExplosions = true;

        for (int i = 0; i < kuvat.Length; i++)
        {
            int alkio = RandomGen.NextInt(kuvat.Length);
            pupujussi.Image = kuvat[alkio];
        }

        PlatformWandererBrain pupujussinAivot = new PlatformWandererBrain();
        pupujussinAivot.Speed = 100;
        pupujussi.Brain = pupujussinAivot;
        pupujussinAivot.FallsOffPlatforms = false;
        pupujussinAivot.JumpSpeed = 300;
        pupujussinAivot.TriesToJump = true;

        Add(pupujussi);
    }


    /// <summary>
    /// Luo pahiksen, jolla on elämäpisteet.
    /// </summary>
    /// <param name="paikka">Pahiksen paikka pelikentällä.</param>
    /// <param name="korkeus">Pahiksen korkeus.</param>
    /// <param name="leveys">Pahiksen leveys.</param>
    public void LuoPahis(Vector paikka, double korkeus, double leveys)
    {
        Pahis pahis = new Pahis(korkeus, leveys, 1);
        Image[] kuvat = LoadImages("ilves", "supikoira", "karhu");
        pahis.Position = paikka;
        pahis.Tag = "pahis";
        pahis.IgnoresExplosions = true;
        
        for (int i = 0; i < kuvat.Length; i++)
        {
            int alkio = RandomGen.NextInt(kuvat.Length);
            pahis.Image = kuvat[alkio];
        }

        PlatformWandererBrain pahiksenAivot = new PlatformWandererBrain();
        pahiksenAivot.Speed = 100;
        pahiksenAivot.FallsOffPlatforms = false;
        pahis.Brain = pahiksenAivot;

        Add(pahis);

        Timer ajastin = new Timer();
        ajastin.Interval = 3.0;
        ajastin.Timeout += delegate ()
        {
            PhysicsObject kranaatti = HeitaKranaatti(pahis);
        };
        ajastin.Start();
        pahis.Destroyed += ajastin.Stop;
    }


    /// <summary>
    /// Pahis heittää kranaatin.
    /// </summary>
    /// <param name="heittaja">Kranaatin heittävä olio.</param>
    /// <returns>Kranaatti.</returns>
    public Grenade HeitaKranaatti(Pahis heittaja)
    {
        Grenade kranaatti = new Grenade(4.0);
        heittaja.Throw(kranaatti, Angle.FromDegrees(40), 10000);
        kranaatti.Explosion.AddShockwaveHandler("pelaaja", KranaattiOsui);
        Add(kranaatti);
        return kranaatti;
    }


    /// <summary>
    /// Törmäyskäsittelijä, kun kranaatti osuu kohteeseen.
    /// </summary>
    /// <param name="rajahdyksenKohde">Räjähdyksen kohde.</param>
    /// <param name="v">Vektori.</param>
    public void KranaattiOsui(IPhysicsObject rajahdyksenKohde, Vector v)
    {
        rajahdyksenKohde.Destroy();
        PelinLopetus();
    }


    /// <summary>
    /// Luo pelikentän.
    /// </summary>
    public void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta.txt");
        kentta.SetTileMethod('h', LuoPelaaja);
        kentta.SetTileMethod('X', LuoEste);
        kentta.SetTileMethod('p', LuoPupujussi);
        kentta.SetTileMethod('i', LuoPahis);
        kentta.Execute(KENTAN_KOKO, KENTAN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.DarkForestGreen, Color.White);
    }


    /// <summary>
    /// Kentän esteet/tasot, joille hypitään.
    /// </summary>
    /// <param name="kohta">Esteen paikka pelikentällä.</param>
    /// <param name="leveys">Esteen leveys.</param>
    /// <param name="korkeus">Esteen korkeus.</param>
    public void LuoEste(Vector kohta, double leveys, double korkeus)
    {
        PhysicsObject este = new PhysicsObject(leveys, korkeus);
        este.Position = kohta;
        este.Color = Color.Brown;
        este.MakeStatic();
        Add(este);
    }


    /// <summary>
    /// Pistelaskuri kerätyille pupujusseille.
    /// </summary>
    public void LuoPistelaskuri()
    {
        pistelaskuri = new IntMeter(0);

        Label pisteet = new Label();
        pisteet.X = Screen.Left + 100;
        pisteet.Y = Screen.Top - 100;
        pisteet.TextColor = Color.Black;
        pisteet.Color = Color.White;
        pisteet.Title = "Pupujusseja saatu: ";

        pistelaskuri.AddTrigger(10, TriggerDirection.Up, PelinLopetus);
        pisteet.BindTo(pistelaskuri);
        Add(pisteet);        
    }


    /// <summary>
    /// Pelin lopetus.
    /// </summary>
    public void PelinLopetus()
    {
        MultiSelectWindow lopetusvalikko = new MultiSelectWindow("Peli päättyi!", "Lopeta peli");
        lopetusvalikko.AddItemHandler(0, Exit);
        Add(lopetusvalikko);
    }


    /// <summary>
    /// Lisää peliin muutamat näppäimet.
    /// </summary>
    public void LisaaNappaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
}