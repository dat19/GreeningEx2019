# 綿毛
綿毛は以下の状態がある。

- Spawn: 発生中。当たり判定なし
- Fly: 上昇中。当たり判定あり
- Hold: 捕まり中。ステラが捕まった時。時間を数える
- Fall: 転落。一定時間が経過。地面の下になったら消す

これをFluffActableで管理する。

## FluffActableを変更
事前にテストで作成したFluffがあるので、これを改造する。

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluff : MonoBehaviour
{
    [SerializeField]
    float removeHight = 5;

    Rigidbody rb;
    float lifeTime;
    float startY;

    public void init(Vector2 vel,float lf)
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = vel;
        lifeTime = lf;
        startY = transform.position.y;
    }

    void Update()
    {
        if(transform.position.y-startY>removeHight)
        {
            Destroy(gameObject);
        }
    }
}
```

- 必要ならnamespaceを追加
- クラス名とファイル名を`FluffActable`に変更
- `Actable`を継承

綿毛を掴む範囲を調べる。綿毛のモデルを表示して、上下のつかめる範囲を調べる。また、ステラが綿毛を掴んでいるポーズの時の、おおよその手の位置を調べて定義しておく。ステラの手の暫定座標の高さが、綿毛を掴める高さの範囲に収まっていれば、掴む処理へ移行する。

## 掴む処理
StellaFluffをアクションスクリプタブルオブジェクトを継承して作成する。これのInitで、ActionBoxにあるオブジェクトを綿毛として記録して、アニメーションの開始などを行う。

まずは、アニメの開始を実装する。
