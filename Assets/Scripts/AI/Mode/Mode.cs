using System.Collections.Generic;

[System.Serializable]
public class Mode
{
    public enum Type
    {
        Cruise,
        Alert,
        PullOver,
        Pass,
        Overtake,
        Draw,
        Transition
    }

    
    public Type type;
    
    private List<CivilianCar> negotiationQueue;

    private List<Type> typeQueue;

    private List<CivilianCar> drawCars;

    private CivilianCar thisCar;

    public CivilianCar proximaCar { get; private set; }


    public Mode(CivilianCar car)
    {
        type = Type.Cruise;
        negotiationQueue = new List<CivilianCar>();
        typeQueue = new List<Type>();
        drawCars = new List<CivilianCar>();
        thisCar = car;
        proximaCar = null;
    }


    public void onEnter(CivilianCar car)
    {
        negotiationQueue.Add(car);
    }


    public void onExit(CivilianCar car)
    {
        negotiationQueue.Remove(car);
    }


    public void update()
    {
        typeQueue.Clear();
        drawCars.Clear();
        proximaCar = null;

        if (thisCar.betweenLanes)
            type = Type.Transition;
        else
        {
            foreach (CivilianCar otherCar in negotiationQueue)
            {
                if (thisCar.onSameRoute(otherCar))
                {
                    float completionIndexGap = thisCar.compareCompletionIndex(otherCar);
                    bool thisCarIsInFront = completionIndexGap > 0;

                    if (thisCarIsInFront)
                        lead(otherCar, completionIndexGap);
                    else
                        follow(otherCar, completionIndexGap);
                }
                else
                    typeQueue.Add(Type.Alert);
            }

            type = Type.Cruise;

            foreach (Type t in typeQueue)
            {
                if ((int)t > (int)type)
                    type = t;
            }

            foreach (CivilianCar drawCar in drawCars)
            {
                if (proximaCar == null)
                    proximaCar = drawCars[0];

                bool drawCarIsBehind = proximaCar.compareCompletionIndex(drawCar) > 0;

                if (drawCarIsBehind)
                    proximaCar = drawCar;
            }
        }
    }


    public void setToTransition()
    {
        type = Type.Transition;
    }


    private void lead(CivilianCar otherCar, float completionIndexGap)
    {
        bool thisCarIsSlow = (thisCar.topSpeed < otherCar.topSpeed);
        bool bothCarsOnSameLane = (thisCar.laneIndex == otherCar.laneIndex);

        if (thisCarIsSlow)
        {
            if (bothCarsOnSameLane)
                typeQueue.Add(Type.PullOver);
            else
                typeQueue.Add(Type.Pass);
        }
        else
        {
            if (!bothCarsOnSameLane)
            {
                bool thisCarIsSufficientlyAhead = completionIndexGap > 1;

                if (!thisCarIsSufficientlyAhead)
                    typeQueue.Add(Type.Overtake);
            }
        }
    }


    private void follow(CivilianCar otherCar, float completionIndexGap)
    {
        bool thisCarIsFast = (thisCar.topSpeed > otherCar.topSpeed);
        bool bothCarsOnSameLane = (thisCar.laneIndex == otherCar.laneIndex);
        bool otherCarIsTransitioning = (otherCar.mode.type == Type.Transition);

        if (thisCarIsFast)
        {
            if (bothCarsOnSameLane || otherCarIsTransitioning)
            {
                typeQueue.Add(Type.Draw);
                drawCars.Add(otherCar);
            }
            else
                typeQueue.Add(Type.Overtake);
        }
        else
        {
            if (!bothCarsOnSameLane && !otherCarIsTransitioning)
            {
                bool thisCarIsSufficientlyBehind = completionIndexGap < -1;

                if (!thisCarIsSufficientlyBehind)
                    typeQueue.Add(Type.Pass);
            }
        }
    }
}
