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
                            .Field(nameof(comment))
              
                    .Build();

        }
        public string Land { get; set; }
        public double Gewicht  { get; set; }

        public double Height { get; set; }

        public double Breite { get; set; }

        public double Tiefe { get; set; }

        public string comment { get; set; }

    }
}