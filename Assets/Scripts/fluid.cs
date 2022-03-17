using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fluid : MonoBehaviour
{
    public float viscosity = 1;
    public float diffusion = 1;
    public float dt = 0.2f;
    public int solverIter = 4;

    public int N = 64;

    float[,] s;
    float[,] density;

    float[,] Vx;
    float[,] Vy;
    float[,] Vx0;
    float[,] Vy0;


    public MeshRenderer planeRenderer;
    private Material planeMat;
    private Texture2D densityTexture;

    private Vector3 mousePrev = new Vector3(0,0,0);


    private void Start()
    {
        s = new float[N, N];
        density = new float[N, N];
        Vx = new float[N, N];
        Vy = new float[N, N];
        Vx0 = new float[N, N];
        Vy0 = new float[N, N];

        planeMat = planeRenderer.sharedMaterial;
        densityTexture = new Texture2D(N, N);
        densityTexture.Apply();
        planeMat.SetTexture("_Texture", densityTexture);
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            int x = (int)((mouse.x) * N);
            int y = (int)((mouse.y) * N);

            Vector3 mouseDrag = mouse - mousePrev;
            mousePrev = mouse;

            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    int xClamp = Mathf.Clamp(x+i, 1, N - 2);
                    int yClamp = Mathf.Clamp(y+j, 1, N - 2);
                    addDensity(xClamp, yClamp, 200);
                    //addVelocity(x, y, Mathf.Clamp(mouseDrag.x * 10000, 0, 100), Mathf.Clamp(mouseDrag.y * 10000, 0, 100));
                    addVelocity(xClamp, yClamp, 0, 10);
                }
            }
        }

        Step();
        draw_dens();
        //draw_vel();
    }

    private void OnMouseDrag()
    {
        Debug.Log("HDGFH");
    }

    private void Step()
    {
        diffuse(1, Vx0, Vx, viscosity);
        diffuse(2, Vy0, Vy, viscosity);

        project(Vx0, Vy0, Vx, Vy);

        advect(1, Vx, Vx0, Vx0, Vy0);
        advect(2, Vy, Vy0, Vx0, Vy0);

        project(Vx, Vy, Vx0, Vy0);
        diffuse(0, s, density, diffusion);
        advect(0, density, s, Vx, Vy);
    }



    private void draw_dens()
    {
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                float value = density[i, j] / 400;
                densityTexture.SetPixel(i, j, new Color(value, value, value));
            }
        }
        densityTexture.Apply();
    }
    private void draw_vel()
    {
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                float valueX = Vx[i, j] / 5;
                float valueY = Vy[i, j] / 5;
                densityTexture.SetPixel(i, j, new Color(valueX, valueY, 0));
            }
        }
        densityTexture.Apply();
    }



    private void addDensity(int x, int y, float amount)
    {
        density[x, y] += amount;
    }
    private void addVelocity(int x, int y, float velX, float velY)
    {
        Vx[x, y] += velX;
        Vy[x, y] += velY;
    }




    void diffuse(int b, float[,] x, float[,] x0, float diff)
    {
        float a = dt * diff * (N-2) * (N-2);
        lin_solve(b, x, x0, a);
    }


    void advect(int b, float[,] d, float[,] d0, float[,] u, float[,] v)
    {
        int i, j, i0, j0, i1, j1;
        float x, y, s0, t0, s1, t1, dt0;
        dt0 = dt * (N-2);
        for (i = 1; i < N-1; i++)
        {
            for (j = 1; j < N-1; j++)
            {
                x = i - dt0 * u[i, j];
                y = j - dt0 * v[i, j];
                if (x < 0.5) x = 0.5f;
                if (x > N + 0.5) x = N + 0.5f; 
                i0 = (int)x;
                i1 = i0 + 1;
                if (y < 0.5) y = 0.5f; 
                if (y > N + 0.5) y = N + 0.5f;
                j0 = (int)y;
                j1 = j0 + 1;

                s1 = x - i0;
                s0 = 1 - s1;
                t1 = y - j0;
                t0 = 1 - t1;

                d[i, j] = s0 * (t0 * d0[i0, j0] + t1 * d0[i0, j1]) + s1 * (t0 * d0[i1, j0] + t1 * d0[i1, j1]);
            }
        }
        set_bnd(b, d);
    }


    void project(float[,] u, float[,] v, float[,] p, float[,] div)
    {
        float h = 1.0f / N;

        for (int i = 1; i < N-1; i++)
        {
            for (int j = 1; j < N-1; j++)
            {
                div[i, j] = -0.5f * h * (u[i + 1, j] - u[i - 1, j] + v[i, j + 1] - v[i, j - 1]);
                p[i, j] = 0;
            }
        }

        set_bnd(0, div); 
        set_bnd(0, p);
        lin_solve(0, p, div, 1);

        for (int i = 1; i < N-1; i++)
        {
            for (int j = 1; j < N-1; j++)
            {
                u[i, j] -= 0.5f * (p[i + 1, j] - p[i - 1, j]) / h;
                v[i, j] -= 0.5f * (p[i, j + 1] - p[i, j - 1]) / h;
            }
        }

        set_bnd(1, u); 
        set_bnd(2, v);
    }


    //void set_bnd(int b, float[,] x)
    //{
    //    for (int i = 1; i < N - 1; i++)
    //    {
    //        x[0, i] = b == 1 ? -x[1, i] : x[1, i];
    //        x[N - 1, i] = b == 1 ? -x[N - 2, i] : x[N - 2, i];
    //        x[i, 0] = b == 2 ? -x[i, 1] : x[i, 1];
    //        x[i, N - 1] = b == 2 ? -x[i, N - 2] : x[i, N - 2];
    //    }

    //    x[0, 0] =           0.33f * (x[1, 0]     + x[0, 1]);
    //    x[0, N - 1] =       0.33f * (x[1, N - 1] + x[0, N - 2]);
    //    x[N - 1, 0] =       0.33f * (x[N - 2, 0]     + x[N - 1, 1]);
    //    x[N - 1, N - 1] =   0.33f * (x[N - 2, N - 1] + x[N - 1, N - 2]);
    //}
    void set_bnd(int b, float[,] x)
    {
        for (int i = 1; i < N - 1; i++)
        {
            x[i, 0] = b == 2 ? -x[i, 1] : x[i, 1];
            x[i, N - 1] = b == 2 ? -x[i, N - 2] : x[i, N - 2];
        }
        for (int j = 1; j < N - 1; j++)
        {
            x[0, j] = b == 1 ? -x[1, j] : x[1, j];
            x[N - 1, j] = b == 1 ? -x[N - 2, j] : x[N - 2, j];
        }

        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, N - 1] = 0.5f * (x[1, N - 1] + x[0, N - 2]);
        x[N - 1, 0] = 0.5f * (x[N - 2, 0] + x[N - 1, 1]);
        x[N - 1, N - 1] = 0.5f * (x[N - 2, N - 1] + x[N - 1, N - 2]);
    }



    void lin_solve(int b, float[,] x, float[,] x0, float a)
    {
        for (int k = 0; k < solverIter; k++)
        {
            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    x[i, j] = ( x0[i, j] + a * (x[i + 1, j] + x[i - 1, j] + x[i, j + 1] + x[i, j - 1] ) ) / (1+4*a);
                }
            }
            set_bnd(b, x);
        }
    }
}
