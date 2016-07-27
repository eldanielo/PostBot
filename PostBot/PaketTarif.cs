using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace PostBot
{
    [Serializable]

    public class PaketTarif
    {

        public static IForm<PaketTarif> BuildForm()
        {


            return new FormBuilder<PaketTarif>(true)
                      .Field(new FieldReflector<PaketTarif>(nameof(Land))
                            .SetActive((state) => state.Land == null))
                             .Field(new FieldReflector<PaketTarif>(nameof(Gewicht))
                            .SetActive((state) => state.Gewicht == 0))
                             .Field(new FieldReflector<PaketTarif>(nameof(Height))
                            .SetActive((state) => state.Height == 0))
                             .Field(new FieldReflector<PaketTarif>(nameof(Tiefe))
                            .SetActive((state) => state.Tiefe == 0))
                            .Field(new FieldReflector<PaketTarif>(nameof(Breite))
                            .SetActive((state) => state.Breite == 0))
                    
              
                    .Build();

        }
        [Prompt("Was ist das Zielland?")]
        public string Land { get; set; }
        [Prompt("Wie viel wiegt das Paket? ")]
        public double Gewicht  { get; set; }
        [Prompt("Wie hoch ist das Paket?")]
        public double Height { get; set; }
        [Prompt("Wie breit ist das Paket?")]
        public double Breite { get; set; }
        [Prompt("Wie tief ist das Paket?")]
        public double Tiefe { get; set; }

    }
}