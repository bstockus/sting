using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {
    public interface IDisplayableHost {

        String Title { get; }

        String SubTitle { get; }

        String Status { get; }

        String Details { get; }

        Boolean IsPaused { get; }

        Boolean IsHostUp { get; }
    }
}
