using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(MeshRenderer))]
public class Art : MonoBehaviour
{
    public Texture2D[] Paintings;

    private MeshRenderer _renderer = null;
    private Material _material = null;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _material = _renderer.material;

        SetArt(Paintings.RandomPick());
        StartCoroutine(Load());
    }

    public void SetArt(Texture texture)
    {
        _material.mainTexture = texture;
    }

    private IEnumerator Load()
    {
        Debug.Log("[e9 Art] Started changing the art.");
        const bool allowExplicit = false;
        const uint fetchAttempts = 1000;
        for (uint i = 0; i < fetchAttempts; i++)
        {
            string baseuri = allowExplicit ? "https://e621.net/" : "https://e926.net/";
            string tags = new string[] { "-text", "-female", "-real" }
                .Select(s => "+" + s)
                .Aggregate((a, b) => a + b);
            string uri = baseuri + "posts.json?limit=1&tags=order:random+type:png+type:jpg+ratio%3A.67" + tags;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.SetRequestHeader("User-Agent", "KTANEModule/Art Appreciation (Local Build) (by BakersDozenBagels)");

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                Match match;
                Regex regex1, regex2, regex3;
                if (!allowExplicit)
                {
                    regex1 = new Regex(@"""rating"":""s""");

                    match = regex1.Match(webRequest.downloadHandler.text);

                    if (!match.Success)
                        continue;
                }

                if (allowExplicit)
                {
                    regex2 = new Regex(@"https:\/\/static1\.e621\.net\/data\/..\/..\/................................\...g");
                    regex3 = new Regex(@"https:\/\/static1\.e621\.net\/data\/sample\/..\/..\/................................\...g");
                }

                else
                {
                    regex2 = new Regex(@"https:\/\/static1\.e926\.net\/data\/..\/..\/................................\...g");
                    regex3 = new Regex(@"https:\/\/static1\.e926\.net\/data\/sample\/..\/..\/................................\...g");
                }

                match = regex2.Match(webRequest.downloadHandler.text);

                if (!match.Success)
                    match = regex3.Match(webRequest.downloadHandler.text);

                if (match.Success)
                {
                    WWW wwwLoader = new WWW(match.Value);
                    yield return wwwLoader;

                    float width = wwwLoader.texture.width, height = wwwLoader.texture.height;

                    GetComponent<Renderer>().material.mainTexture = wwwLoader.texture;

                    Debug.Log("[e9 Art] Changed the art.");

                    break;
                }

                yield return new WaitForSeconds(1f);
            }
        }
    }
}
