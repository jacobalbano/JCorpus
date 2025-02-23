﻿using Common;
using Common.Addins.Search;
using Common.Configuration;
using Common.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Implementation.Search;

#if DEBUG
internal class DebugSearch : ISearch
{
    public IEnumerable<SearchHit> YieldHitsInWork(ICorpus corpus, CorpusWork work)
    {
        yield return new SearchHit(work.UniqueId, "1", jpLipsum[Math.Abs(work.UniqueId.GetHashCode()) % jpLipsum.Length], Array.Empty<SearchMatchRange>());
    }

    private static readonly string[] jpLipsum = @"しかし私は、喜びを非難して苦痛を賞賛するという誤ったこの考えがすべてどのようにして誕生したかをあなたに説明しなければならないから、私はあなたにその体系を完璧に説明し、真実を求める偉大な探究家、人間の喜びを築く建築家の実践的な教えを詳しく説明しよう。だれも喜びそのものを、それが喜びであるという理由で拒んだり、嫌ったり、避けたりはしない。しかし、どのようにして喜びを理性的に追求するかを知らない人たちは非常に苦痛な結末に直面する。同様に、苦痛そのものを、それが苦痛であるという理由で愛したり、探したり、手に入れることを望んだりする者もいない。しかし、ときには苦労や苦痛がその人に大いなる喜びをいくらかもたらす状況がおこることがある。些末な例を挙げると、私たちのうちのだれが、そこから何か有益なものを得られないのに、骨の折れる肉体運動を引き受けるだろうか？しかしだれに、いらだたしい結末のない喜びを享受することを選ぶ人や、その結果としての喜びを生み出さないような痛みを避ける人にある、落ち度を見つける権利はあるのだろうか？一方、わたしたちは正当な憤りをもって批判する。そして、今の喜びの魅力にだまされてあまりにもやる気を失い、欲望によってあまりにも盲目的になってしまうことで、その後に起こるべき苦痛や困難を予想できなくなってしまう人たちを嫌う；そして、同等の非難は意志の弱さによって自身の務めに失敗する人たちにも当てはまる。これは苦労から苦痛へと縮小することと等しい。これらの場合は完全に単純で、見分けるのはたやすい。自由なときにおいて、選択の力に制約がなく、いちばん好きなものを選ぶのに一切の妨げがないとき、あらゆる喜びが受け入れられ、あらゆる苦痛が避けられる。しかし、ある特定の状況において務めや義務により、しばしば喜びを拒み、いらだたしいことを受け入れなければならないことが起こる。賢い人間はそれゆえ常に、この選択の原則によってこれらの問題を制する：賢い人間は他のより大きな喜びを確実に手に入れるためならば喜びを拒み、あるいはよりひどいものを避けるために苦痛に耐える。"
        .Split("？！。".ToCharArray());
}
#endif