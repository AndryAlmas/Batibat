using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using System.Web.Hosting;
using Microsoft.Ajax.Utilities;
using System.Collections.ObjectModel;

namespace Batibatlocation.Helpers
{
    public static class SeleniumHelper
    {
        public static readonly int minValue = 2500;
        public static readonly int maxValue = 5000;
        public static readonly int timeoutSecondi = 20;
        public static IWebDriver GetDriver()
        {
            string chromeDriverService = HostingEnvironment.MapPath("~/Drivers");
            var options = new ChromeOptions();
            options.AddArgument("user-data-dir=C:/Users/andry/AppData/Local/Google/Chrome/User Data");
            options.AddArgument("--start-maximized");
            options.AddExcludedArgument("enable-automation");

            return new ChromeDriver(chromeDriverService, options);
        }

        public static void RicercaGruppiCostruzione(int? groupsMaxLimit = null)
        {
            using (var driver = SeleniumHelper.GetDriver())
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/groups");

                // Inserisci la keyword nel campo di ricerca
                var searchBox = TrovaElementoConRetryCss(driver, "input[placeholder='Rechercher des groupes']");
                searchBox.SendKeys("Construction");
                searchBox.SendKeys(Keys.Enter);

                // (Opzionale) clicca su "Gruppi vicino a te"
                var groups = TrovaElementoConRetry(driver, "//a[contains(@href, '/groups/search/groups/?q=Construction')]");
                groups.Click();

                var checkbox = TrovaElementoConRetryCss(driver, "input[aria-label='Près de moi']");
                checkbox.Click();


                // Itera i risultati e salva gli URL
                var groupLinks = TrovaElementiConRetryCss(driver, "a[href^='https://www.facebook.com/groups/']", groupsMaxLimit);

                var listaGruppi = new List<string>();
                foreach (var gruppo in groupLinks)
                {
                    var url = gruppo.GetAttribute("href");
                    listaGruppi.Add(url);
                    // Qui puoi chiamare il metodo che pubblica un post
                }
                var a = listaGruppi;
            }
        }

        public static void AttesaUmana()
        {
            Random random = new Random();
            int numeroCasuale = random.Next(minValue, maxValue);
            Thread.Sleep(numeroCasuale);
        }

        public static IWebElement TrovaElementoConRetry(IWebDriver driver, string xpath)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (watch.Elapsed.TotalSeconds < timeoutSecondi)
            {
                try
                {
                    var element = driver.FindElement(By.XPath(xpath));
                    if (element != null)
                        return element;
                }
                catch (NoSuchElementException)
                {
                    AttesaUmana();
                }
            }

            throw new TimeoutException($"Elemento con XPath '{xpath}' non trovato dopo {timeoutSecondi} secondi.");
        }

        public static IWebElement TrovaElementoConRetryCss(IWebDriver driver, string cssSelector)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (watch.Elapsed.TotalSeconds < timeoutSecondi)
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector(cssSelector));
                    if (element != null)
                        return element;
                }
                catch (NoSuchElementException)
                {
                    AttesaUmana();
                }
            }

            throw new TimeoutException($"Elemento con CSS Selector '{cssSelector}' non trovato dopo {timeoutSecondi} secondi.");
        }


        public static List<IWebElement> TrovaElementiConRetryCss(IWebDriver driver, string cssSelector, int? groupsMaxLimit = null)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<IWebElement> listGroups = new List<IWebElement>();

            if (groupsMaxLimit != null)
            {
                while (watch.Elapsed.TotalSeconds < groupsMaxLimit +10) // un minimo di 10 secondi per limiti bassi
                {
                    try
                    {
                        AttesaUmana();
                        var elements = driver.FindElements(By.CssSelector(cssSelector));
                        if (elements != null)
                        {
                            foreach (var item in elements)
                            {
                                var url = item.GetAttribute("href");
                                if (!url.Contains('?'))
                                {
                                    if (!listGroups.Contains(item) && listGroups.Count < groupsMaxLimit)
                                        listGroups.Add(item);
                                }
                            }
                            if (listGroups.Count >= groupsMaxLimit)
                                return listGroups;
                            else
                            {
                                // Scrolla in basso
                                ScrollaVersoIlBassoConAttesa(driver);
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        AttesaUmana();
                    }
                }

                throw new TimeoutException($"Elemento con CSS Selector '{cssSelector}' non trovato dopo {timeoutSecondi} secondi.");

            }
            else
            {
                while (watch.Elapsed.TotalSeconds < timeoutSecondi)
                {
                    try
                    {
                        AttesaUmana();
                        var elements = driver.FindElements(By.CssSelector(cssSelector));
                        if (elements != null)
                        {
                            listGroups.AddRange(elements);
                            return listGroups;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        AttesaUmana();
                    }
                }

                throw new TimeoutException($"Elemento con CSS Selector '{cssSelector}' non trovato dopo {timeoutSecondi} secondi.");

            }
        }

        private static void ScrollaVersoIlBassoConAttesa(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0, window.innerHeight);");
            Thread.Sleep(new Random().Next(800, 1500)); // breve attesa casuale
        }


    }
}