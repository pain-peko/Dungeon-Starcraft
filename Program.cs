﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Dungeon_Starcraft
{
    class Program
    {
        static void Main(string[] args)
        {
            var Game = new Game();

            var MainHero = new MainHero("MainHero", 100, 0, 2, 20,100,0);
            var Boss = new Boss("Boss", 100, 0, 5, 5, Game.Map.Size - 1);


            Game.BattleEventArgs.MainHero = MainHero;
            Game.OnBattleEvent += Game_OnBattleEvent;


            using (StreamReader sr = new StreamReader("saves/savelog.txt"))
            {
                if (sr.ReadLine() == null)
                {
                    Console.WriteLine("1 - Начать новую игру\n3 - Выйти из игры");
                }
                else
                {
                    Console.WriteLine("1 - Начать новую игру\n2 - Загрузить игру\n3 - Выйти из игры");
                }
            }
            int menu = Convert.ToInt32(Console.ReadLine());
            Console.Clear();

            if (menu == 1)
                Game.Start(ref MainHero,ref Boss);
            else if (menu == 2)
                Game.Load(AskSaveName(),ref MainHero,ref Boss);
            else if (menu == 3)
                Environment.Exit(0);

            while (!Game.End)
            {
                MainHero.ShowStatus();
                Console.WriteLine("-------------------------------------------");
                if (Game.Map[Game.Map.Size - 2] != MainHero) //Обычная ходьба по подземелью
                {
                    Console.WriteLine("Ваши действия:\n1 - Идти вперед\n8 - Сохраниться\n9 - Выйти из игры");
                    int turn = Convert.ToInt32(Console.ReadLine());
                    Console.Clear();


                    if (turn == 1)
                    {
                        if (Game.Map[MainHero.Location + 1] is Loot)
                        {
                            if (Game.Map[MainHero.Location + 1] is Chest)
                            {
                                Chest temp = (Chest)Game.Map[MainHero.Location + 1];
                                Console.Write($"Вы нашли золото!\n+{temp.Gold} золота\n");
                                Console.WriteLine("-------------------------------------------");
                                MainHero.Gold += temp.Gold;
                                Game.Map[temp.Location] = MainHero;
                                MainHero.Location++;
                            }
                            else if (Game.Map[MainHero.Location + 1] is Aura)
                            {
                                if (Game.Map[MainHero.Location + 1] is AttackAura)
                                {
                                    AttackAura temp = (AttackAura)Game.Map[MainHero.Location + 1];
                                    Console.Write($"Вы обнаружили алтарь древних Богов Войны!\n+{temp.AttackBuff} к урону\n");
                                    Console.WriteLine("-------------------------------------------");
                                    MainHero.Damage += temp.AttackBuff;
                                    Game.Map[temp.Location] = MainHero;
                                    MainHero.Location++;
                                }
                            }
                        }
                        else if (Game.Map[MainHero.Location + 1] is Unit)
                        {
                            Unit temp = (Unit)Game.Map[MainHero.Location + 1];
                            Console.WriteLine($"Вы встретились с {temp.Name}!");
                            Console.WriteLine("-------------------------------------------");
                            Game.BattleEventArgs.Enemy = temp;
                            Game.Meeting(temp, Game.BattleEventArgs);

                            Game.Map[temp.Location] = MainHero;
                            MainHero.Location++;
                        }
                    }
                    else if (turn == 8)
                    {
                        Console.WriteLine("Выберите название для сохранения");
                        string savename = Console.ReadLine();
                        Game.Save(savename,MainHero,Boss);
                    }
                    else if (turn == 9)
                    {
                        Environment.Exit(0);
                    }


                }
                else //Встреча с боссом
                {
                    Console.Write("Впереди босс...\nВаши действия:\n1 - Идти вперед\n2 - Отдохнуть, восстановив здоровье, но дав боссу времени на приготовления\n");
                    int turn = Convert.ToInt32(Console.ReadLine());
                    Console.Clear();
                    if (turn == 2)
                    {
                        var rnd = new Random();
                        int temp = rnd.Next(5, 11);
                        Console.WriteLine($"Вы полностью восстановились, но {Boss.Name} увеличил свою броню на {temp}");
                        Console.WriteLine("-------------------------------------------");
                        Boss.Armor += temp;
                        MainHero.HP = 100; //Изменить на максимум
                    }
                    Console.WriteLine($"Вы встретились с {Boss.Name}!");
                    Game.BattleEventArgs.Enemy = Boss;
                    Game.Meeting(Boss, Game.BattleEventArgs);

                    Game.Map[Boss.Location] = MainHero;
                    MainHero.Location++;
                    Game.End = true;
                }
            }
        }
        static void Game_OnBattleEvent(object sender, BattleEventArgs e)
        {
            if (sender is Game)
            {
                e.MainHero.ShowStatus();
                Console.WriteLine("-------------------------------------------");
                e.Enemy.ShowStatus();
                Console.WriteLine("-------------------------------------------");
                while (e.MainHero.HP > 0 && e.Enemy.HP > 0)
                {
                    Console.Write("Ваши действия:\n1 - Атака!\n");
                    int turn = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("-------------------------------------------");
                    e.Enemy.HP -= (e.MainHero.Damage - e.Enemy.Armor);
                    Console.Write($"Вы атаковали {e.Enemy.Name} и нанесли {e.MainHero.Damage - e.Enemy.Armor} урона!\n");
                    Console.WriteLine("-------------------------------------------");
                    e.Enemy.ShowStatus();
                    Console.WriteLine("-------------------------------------------");
                    e.MainHero.HP -= (e.Enemy.Damage - e.MainHero.Armor);
                    Console.Write($"Вас атаковал {e.Enemy.Name} и нанес {e.Enemy.Damage - e.MainHero.Armor} урона!\n");
                    Console.WriteLine("-------------------------------------------");
                    e.MainHero.ShowStatus();
                }
                if (e.Enemy.HP < 1)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.Clear();
                    Console.WriteLine($"Победа! {e.Enemy.Name} пал...");
                    Console.WriteLine("-------------------------------------------");
                }
                else if (e.MainHero.HP < 1)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.Clear();
                    Console.WriteLine($"Вы были убиты! {e.Enemy.Name} победил!");
                    Game.End = true;
                }
            }
        }
        static string AskSaveName ()
        {
            int saveNumber;
            using (StreamReader sr = new StreamReader("saves/savelog.txt"))
            {
                if (sr.ReadLine() == null)
                {
                    throw new ArgumentException("Сохранений не существует!\nНельзя позволять пользователю загружать игру, если он ни разу не сохранялся\n");
                }
                sr.DiscardBufferedData();
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; !sr.EndOfStream; i++)
                {
                    Console.WriteLine($"{i + 1} - {sr.ReadLine()}");
                }
                Console.Write("Выбери номер сохранения - ");
                saveNumber = Convert.ToInt32(Console.ReadLine());
                Console.Clear();
            }
            return Game.GetSaveName(saveNumber);
        }
    }
}
