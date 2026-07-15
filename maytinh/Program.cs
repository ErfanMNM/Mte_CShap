Console.WriteLine("Hello, World!");
string dauvao = "1+2*8/2/1-1-2*6--3";//tính theo thứ tự ưu tiên: /, *, +, -

string[] phepcong = dauvao.Split('+');

double maincong = 0;//ket qua phep cong

for (int i = 0; i < phepcong.Length; i++)
{

    string[] pheptru = phepcong[i].Split('-');
    double maintru = 0;//ket qua phep tru

    for (int j = 0; j < pheptru.Length; j++)
    {
        string[] phepnhan = pheptru[j].Split('*');

        double mainnhan = 0;//ket qua phep nhan

        for (int k = 0; k < phepnhan.Length; k++)
        {
            string[] phepchia = phepnhan[k].Split('/');
            double mainchia = 0;//ket qua phep chia


            for (int l = 0; l < phepchia.Length; l++)
            {
                if(l == 0)
                {
                    mainchia = Convert.ToDouble(phepchia[l]);
                }
                else
                {
                    mainchia /= Convert.ToDouble(phepchia[l]);
                }
            }

            if(k == 0)
            {
                mainnhan = mainchia;
            }
            else
            {
                mainnhan *= mainchia;
            }
        }

        if(j == 0)
        {
            maintru = mainnhan;
        }
        else
        {
            maintru -= mainnhan;
        }
    }

    if(i == 0)
    {
        maincong = maintru;
    }
    else
    {
        maincong += maintru;
    }

}

string result = $@"Kết quả: " + maincong.ToString();

Console.WriteLine(result);

while (true);