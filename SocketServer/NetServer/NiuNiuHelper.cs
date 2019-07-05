using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.NetServer
{
    public class NiuNiuHelper
    {
        public static NiuNiuType CalcNNType(int[] cards)
        {
            int[] tempcards = new int[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                tempcards[i] = cards[i] % 100;
            }

            List<int> lcards = tempcards.ToList();
            lcards.Sort();

            if (lcards[0] > 10)
                return NiuNiuType.五花牛;

            if (lcards[lcards.Count - 1] <= 5)
            {
                int allsum = 0;
                for (int i = 0; i < lcards.Count; i++)
                {
                    allsum += lcards[i];
                }

                if (allsum <= 10)
                    return NiuNiuType.五小牛;
            }

            //吧  J Q K 变为10
            int allSum = 0;
            for (int i = 0; i < lcards.Count; i++)
            {
                lcards[i] = lcards[i] > 10 ? 10 : lcards[i];
                allSum += lcards[i];
            }

            //总和去除两张牌 剩下是10的倍数就有牛
            for (int i = 0; i < lcards.Count - 1; i++)
            {
                for (int j = i + 1; j < lcards.Count; j++)
                {
                    int nLeftSum = allSum - lcards[i] - lcards[j];

                    if (nLeftSum % 10 == 0)
                    {
                        int nnType = (lcards[i] + lcards[j]) % 10;
                        if (nnType == 0)
                            return (NiuNiuType)10;
                        else
                            return (NiuNiuType)nnType;
                    }
                }
            }
            return 0;
        }

        public static int GetMaxCard(int[] cards)
        {
            int n = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] % 100 > n % 100)
                {
                    n = cards[i];
                }
                else if(cards[i] % 100 == n % 100)
                {
                    if (cards[i] / 100 > n / 100)
                    {
                        n = cards[i];
                    }
                }
            }
            return n;
        }

        public static int CalcPower(NiuNiuPlayer p, NiuNiuPlayer phost)
        {
            int winType = 0;
            int winPower = 0;
            if ((int)p.NNType == (int)phost.NNType)//比较牛几
            {
                winType = (int) p.NNType;
                if (p.MaxCard % 100 == phost.MaxCard % 100) //比较点数
                {
                    if (p.MaxCard / 100 > phost.MaxCard / 100) //比较花色
                    {
                        winPower = 1;
                    }
                    else
                    {
                        winPower = -1;
                    }
                }
                else
                {
                    if (p.MaxCard % 100 > phost.MaxCard % 100) //比较点数
                    {
                        winPower = 1;
                    }
                    else
                    {
                        winPower = -1;
                    }
                }
            }
            else
            {
                if ((int)p.NNType > (int)phost.NNType)
                {
                    winType = (int)p.NNType;
                    winPower = 1;
                }
                else
                {
                    winType = (int)phost.NNType;
                    winPower = -1;
                }
            }


            switch ((int)winType)
            {
                case 7:
                    winPower = winPower * 2;
                    break;
                case 8:
                    winPower = winPower * 2;
                    break;
                case 9:
                    winPower = winPower * 2;
                    break;
                case 10:
                    winPower = winPower * 3;
                    break;
                case 11:
                    winPower = winPower * 5;
                    break;
                case 12:
                    winPower = winPower * 8;
                    break;
            }

            return winPower;
        }
    }
}
