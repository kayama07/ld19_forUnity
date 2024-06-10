using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 1秒後にDestroyメソッドを呼び出して、自身を破棄する
        Invoke("DestroySelf", 0.1f);
    }

    // 自身を破棄するメソッド
    void DestroySelf()
    {
        Destroy(gameObject);
    }
}

