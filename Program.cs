using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Raylib_cs;
using static Raylib_cs.Raylib;
namespace Search_for_Honey;
public class Program
{
    private static bool started;
    private static bool replace = false;
    private static int timer = 0;
    private static bool collided = false;
    private const int Scale = 4;
    private const int Columncount = 3;
    private static float acceleration;
    private static float y;
    private static Random Rnd;
    private static readonly (float, bool)[] Columns = new (float, bool)[4];
    private static int wheight = 0;
    private static int columnspan;
    private static int gamestate = 0;
    private static Image icon;
    private static Texture2D bee;
    private static Texture2D beesmol;
    private static Texture2D ground;
    private static Texture2D wall;
    private static Texture2D honey;
    private static Texture2D clouds1;
    private static Texture2D clouds2;
    private static Texture2D clouds3;
    private static Texture2D gameoverscreen;
    private static Texture2D title;
    private static Texture2D logo1;
    private static Sound jump;
    private static Sound hurt;
    private static Sound grow;
    private static Sound kaching;
    private static Sound ending;
    private static Sound logo;
    private static Music stagemusic;
    private static Music menu;
    private static float oldx = 0;
    private static int score = 0;
    private static int maxscore = 0;
    private const float Width = 800;
    private const float Height = 600;
    private static RenderTexture2D rtex;
    private static Font font;
    private static Font opensans;
    private static int cooldown;
    private static bool smol;

    public static float Smoothstep(float x)
    {
        return x* x*(3 - 2 * x);
    }
    public static void Main()
    {
        
        Rnd = new Random();
        InitWindow((int)Width, (int)Height, Resource1.SearchForHoney);
        icon = LoadImage(Path.Combine(Resource1.images, "honey.png"));
        SetWindowIcon(icon);
        SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);
        SetTargetFPS(60);
        InitAudioDevice();
        bee = LoadTexture(Path.Combine(Resource1.images, "bee_strip9.png"));
        beesmol = LoadTexture(Path.Combine(Resource1.images, "smol bee_strip9.png"));
        ground = LoadTexture(Path.Combine(Resource1.images, "ground.png"));
        wall = LoadTexture(Path.Combine(Resource1.images, "wall.png"));
        honey = LoadTexture(Path.Combine(Resource1.images, "honey.png"));
        gameoverscreen = LoadTexture(Path.Combine(Resource1.images, "gameover.png"));
        title = LoadTexture(Path.Combine(Resource1.images, "title.png"));
        clouds1 = LoadTexture(Path.Combine(Resource1.images, "clouds1.png"));
        clouds2 = LoadTexture(Path.Combine(Resource1.images, "clouds2.png"));
        clouds3 = LoadTexture(Path.Combine(Resource1.images, "clouds3.png"));
        logo1 = LoadTexture(Path.Combine(Resource1.images, "logo.png"));
        jump = LoadSound(Path.Combine(Resource1.audio,"jump.mp3"));
        hurt = LoadSound(Path.Combine(Resource1.audio, "hurt.mp3"));
        grow = LoadSound(Path.Combine(Resource1.audio, "grow.mp3"));
        kaching = LoadSound(Path.Combine(Resource1.audio, "kaching.mp3"));
        ending = LoadSound(Path.Combine(Resource1.audio, "ending.mp3"));
        logo = LoadSound(Path.Combine(Resource1.audio, "logo.mp3"));
        stagemusic = LoadMusicStream(Path.Combine(Resource1.audio, "stage.mp3"));
        menu = LoadMusicStream(Path.Combine(Resource1.audio, "bgm.mp3"));
        wheight = wall.height;
        columnspan = (int)((Width + 2 * wall.width) / Columncount);
        rtex = LoadRenderTexture((int)Width, (int)Width);
        font = LoadFontEx(Resource1.font, 115, null, 0);
        opensans = LoadFontEx(Resource1.opensans, 55, null, 0);
        GenTextureMipmaps(ref font.texture);
        SetTextureFilter(font.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
        GenTextureMipmaps(ref opensans.texture);
        SetTextureFilter(opensans.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
        gamestate = 4;

        var bgcolor = new Color(178, 198, 255, 255);
        Reset();
        PlayMusicStream(stagemusic);
        PlayMusicStream(menu);
        while (!WindowShouldClose())
        {
            BeginTextureMode(rtex);
            ClearBackground(bgcolor);
            switch (gamestate)
            {
                case 0:
                    Start();
                    break;
                case 1:
                    Stage();
                    break;
                case 2:
                    End();
                    break;
                case 4:
                    Logo();
                    break;
            }
            EndTextureMode();
            BeginDrawing();
            ClearBackground(gamestate == 4 ? Color.BLACK : bgcolor);
            if (IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                if (IsWindowFullscreen())
                {
                    ToggleFullscreen();
                    SetWindowSize((int)Width, (int)Height);
                }
                else
                {
                    var monitor = GetCurrentMonitor();
                    var x = GetMonitorWidth(monitor);
                    var y = GetMonitorHeight(monitor);
                    SetWindowSize(x, y);
                    ToggleFullscreen();
                }
            }

            var aspect = (1.0f * GetScreenHeight() / GetScreenWidth()) / 0.75f;
            var srect = new Rectangle(0, (float)rtex.texture.height - Height, (float)rtex.texture.width, -Height);
            var drect = new Rectangle(GetScreenWidth() * (1 - aspect) * 0.5f, 0, GetScreenWidth() * aspect, GetScreenHeight());
            DrawTexturePro(rtex.texture, srect, drect, Vector2.Zero, 0, Color.WHITE);
            EndDrawing();
        }
        CloseAudioDevice();
        CloseWindow();
        UnloadImage(icon);
        UnloadTexture(bee);
        UnloadTexture(beesmol);
        UnloadTexture(ground);
        UnloadTexture(wall);
        UnloadTexture(honey);
        UnloadTexture(gameoverscreen);
        UnloadTexture(title);
        UnloadTexture(clouds1);
        UnloadTexture(clouds2);
        UnloadTexture(clouds3);
        UnloadTexture(logo1);
        UnloadMusicStream(stagemusic);
        UnloadMusicStream(menu);
        UnloadSound(ending);
        UnloadSound(jump);
        UnloadSound(hurt);
        UnloadSound(grow);
        UnloadSound(logo);
        UnloadSound(kaching);
        UnloadRenderTexture(rtex);
        UnloadFont(font);
        UnloadFont(opensans);
    }

    private static void Logo()
    {
        if (360 < timer)
            gamestate = 0;

        if (timer == 0)
            PlaySound(logo);

            ClearBackground(timer < 120 || 240 < timer ? Color.BLACK : Color.WHITE);

        timer++;  
        var fract = Smoothstep(MathF.Min(1f, MathF.Max(0f, timer / 120f)));
        var fract2 = Smoothstep(MathF.Min(1f, MathF.Max(0f, (timer - 120) / 120f)));
        var fade = Smoothstep(MathF.Min(1f,MathF.Max(0f,(360f - timer) / 120)));

        var startposx = Width / 2 - logo1.width / 2;
        var startposy = Height/ 2 - logo1.height / 2;
        var color = new Color(255, 255, 255, (int)(255*fract * fade));
        var color2 = new Color(203, 206, 249, (int)(255 * fract2 * fade));

        Raylib.DrawTexture(logo1,(int) startposx,(int) startposy, color);
        DrawTextEx(opensans , Resource1.Presents, new Vector2((int)(Width / 2 - MeasureTextEx(font, Resource1.Presents, 55, 0).X / 2), (int)(Height * 0.70f)), 55, 0, color2);

    }
    private static void Start()
    {
        var character = smol ? beesmol : bee;
        var framewith = (int)(character).width / 9f;

        if (IsKeyPressed(KeyboardKey.KEY_SPACE))
        {
            gamestate = 1;
            Reset();
        }
        else
        {
            UpdateMusicStream(menu);

            var cloudposition = new Vector2(0, Height * 0.35f);
            var cloudsource = new Rectangle((int)((timer / 8f) % clouds1.width), 0, Width, clouds1.height);
            DrawTextureRec(clouds1, cloudsource, cloudposition, Color.WHITE);
            cloudsource = new Rectangle((int)((timer / 4f) % clouds2.width), 0, Width, clouds2.height);
            DrawTextureRec(clouds2, cloudsource, cloudposition, Color.WHITE);
            cloudsource = new Rectangle((int)((timer / 2f) % clouds3.width), 0, Width, clouds3.height);
            DrawTextureRec(clouds3, cloudsource, cloudposition, Color.WHITE);
            DrawRectangle(0, (int)(Height * 0.65), (int)Width, (int)(Height * 0.35f), Color.WHITE);

            if ((timer / 20) % 2 == 0)
                DrawTextEx(font, Resource1.PressSpaceToStart, new Vector2((int)(Width / 2 - MeasureTextEx(font, Resource1.PressSpaceToStart, 35, 0).X / 2), (int)(Height * 0.8f)), 35, 0, Color.YELLOW);

            DrawTextureEx(title, new Vector2((Width - title.width) / 2.0f,0), 0, 1.0f, Color.WHITE);

            y = MathF.Sin(timer * 0.0312f) * 50f;
            var x = MathF.Sin(timer * 0.00944f) * Width * .4f;
            var reverse = x < oldx;
            var playerposition = new Vector2(Width * 0.5f - 27 + x, Height * 0.4f - 47 + y);
            var source = new Rectangle((timer % 9) * framewith - (reverse ? framewith : 0), 0, (reverse ? -1 : 1) * framewith, character.height);
            DrawTextureRec(character, source, playerposition, Color.WHITE);
            oldx = x;
            timer++;
        }

    }

    private static void End()
    {
        smol = false;
        if (IsKeyPressed(KeyboardKey.KEY_R))
        {
            StopMusicStream(stagemusic);
            PlayMusicStream(stagemusic);
            gamestate = 1;
            Reset();
        }
        else if (IsKeyPressed(KeyboardKey.KEY_B))
        {
            StopMusicStream(menu);
            PlayMusicStream(menu);
            StopSound(ending);
            gamestate = 0;
        }
        else
        {
            DrawTextureEx(gameoverscreen, Vector2.Zero, 0, Width / gameoverscreen.width, Color.WHITE);
            DrawTextEx(font, Resource1.PressRToRestart, new Vector2((Width * 0.53f), (Height - 165)), 35, 0, ((timer / 30) % 2 == 0) ? Color.YELLOW : Color.ORANGE);
            DrawTextEx(font, Resource1.PressBToReturnToMenu, new Vector2((Width * 0.53f), (Height - 130)), 35, 0, ((timer / 30) % 2 == 1) ? Color.YELLOW : Color.ORANGE);
            DrawTextEx(font, string.Format(Resource1.Score, score), new Vector2((Width * 0.53f), (Height - 95)), 35, 0, Color.YELLOW);
            DrawTextEx(font, string.Format(Resource1.MaxScore, maxscore), new Vector2((Width * 0.53f), (Height - 60)), 35, 0, Color.YELLOW);

            timer++;
        }
    }

    private static void Stage()
    {
        var character = smol ? beesmol : bee;
        var framewith = (int)(character.width / 9f);

        UpdateMusicStream(stagemusic);
        if (IsKeyDown(KeyboardKey.KEY_SPACE))
        {
            started = true;
            acceleration = -Scale;
            PlaySound(jump);
        }
        if (collided)
        {
            collided = false;
            if (!smol && cooldown == 0)
            {
                cooldown = timer;
                smol = true;
                PlaySound(hurt);
            }
            if (smol && cooldown == 0)
            {
                if (score > maxscore)
                {
                    maxscore = score;
                }

                PlaySound(ending);
                gamestate = 2;
            }
        }
        const float sch = Height / 2.0f;
        var groundposition = new Vector2(0, Height - ground.height);
        var playerposition = new Vector2(Width / 2.0f - 27, sch - 47 + y);
        var playerbounds = new Rectangle(playerposition.X + 2, playerposition.Y + 2, framewith - 4f, character.height - 4f);
        if (playerposition.Y + playerbounds.height > groundposition.Y)
            collided = true;
        y = Math.Min(y, groundposition.Y - playerbounds.height + 47 - sch);
        y = Math.Max(-250, y);
        var cloudposition = new Vector2(0, Height * 0.35f);
        var cloudsource = new Rectangle((int)((timer / 8f) % clouds1.width), 0, Width, clouds1.height);
        DrawTextureRec(clouds1, cloudsource, cloudposition, Color.WHITE);
        cloudsource = new Rectangle((int)((timer / 4f) % clouds2.width), 0, Width, clouds2.height);
        DrawTextureRec(clouds2, cloudsource, cloudposition, Color.WHITE);
        cloudsource = new Rectangle((int)((timer / 2f) % clouds3.width), 0, Width, clouds3.height);
        DrawTextureRec(clouds3, cloudsource, cloudposition, Color.WHITE);
        DrawRectangle(0, (int)(Height * 0.65), (int)Width, (int)(Height * 0.35), Color.WHITE);
        for (var i = 0; i < Columncount; i++)
        {
            var (y1, ishoney) = Columns[i];
            y1 = -y1;
            var y2 = Height * 0.8f - Columns[i].Item1;
            var x = columnspan * (i + 1) - timer % (columnspan) - wall.width;
            if (i == 0 && x == -wall.width + 1)
                replace = true;
            DrawTexture(wall, x, (int)y1, Color.WHITE);

            if (ishoney)
            {
                var honeyx = x - 10;
                var honeyy = (int)y1 + 350;
                DrawTexture(honey, honeyx, honeyy, Color.WHITE);
                if (CheckCollisionRecs(new Rectangle(honeyx, honeyy, honey.width, honey.height), playerbounds))
                {
                    if (smol)
                    {
                        smol = false;
                        PlaySound(grow);
                    }
                    else
                    {
                        score += 10;
                        PlaySound(kaching);
                    }

                    Columns[i] = (Columns[i].Item1, false);
                }
            }

            DrawTexture(wall, x, (int)y2, Color.WHITE);

            if (CheckCollisionRecs(new Rectangle(x, y1, wall.width, wall.height), playerbounds) ||
                CheckCollisionRecs(new Rectangle(x, y2, wall.width, wall.height), playerbounds))
                collided = true;
        }
        if (replace)
        {
            ReplaceColumn();
            replace = false;
            score++;
        }
        var source = new Rectangle(timer % ground.width, 0, Width, ground.height);
        DrawTextureRec(ground, source, groundposition, Color.WHITE);
        source = new Rectangle((timer % 9) * framewith, 0, framewith, character.height);
        if (cooldown == 0 || (timer / 2) % 2 == 0)
            DrawTextureRec(character, source, playerposition, Color.WHITE);
        var text = String.Format(Resource1.Score, score);
        DrawTextEx(font, text, new Vector2(((Width - MeasureText(text, 35)) / 2), (Height - 50)), 35, 0, Color.YELLOW);

        if (!started) return;
        timer++;
        acceleration += 0.1f * Scale;
        y += acceleration;
        if (cooldown != 0 && cooldown + 60 < timer)
            cooldown = 0;
    }
    private static void Reset()
    {
        smol = false;
        Rnd = new Random();
        score = 0;
        collided = false;
        started = false;
        timer = 0;
        acceleration = 0;
        y = 0;
        for (var i = 0; i <= Columncount; i++)
            Columns[i] = (Rnd.Next(0, (int)(wheight * 0.9)), Rnd.Next(0, 20) < 3);
    }
    private static void ReplaceColumn()
    {
        for (var i = 0; i <= Columncount - 1; i++)
            Columns[i] = Columns[i + 1];
        Columns[Columncount - 1] = (Rnd.Next(0, (int)(wheight * 0.9)), Rnd.Next(0, 20) < 3);
    }
}