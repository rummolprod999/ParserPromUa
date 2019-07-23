namespace PromUa

module S =
    type T =
        {
          TmpP : string
          Pref : string
          ConS : string
        }

    type PromUaUser =
        {
            User : string
            Pass : string
        }
    let mutable argTuple = Argument.Nan
    let mutable logFile = ""
    let mutable Settings = {
        TmpP = ""
        Pref = ""
        ConS = ""
    }
    let mutable User = {
        User = ""
        Pass = ""
    }