using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genom
{
    /**
     * Genome Skils:
     *  Carnivore (плотоядность): эффективность усвоения мяса
     *  Herbivore (травоядность): эффетивность усвоения обычной еды
     *                   Offence: наносимый урон противнику - урон вырывает кусочек еды из тела. он летит в случайную сторону и может быть съеден только через X времени Если одна клетка атакует вторую, урон получают обе
     *                  Deffence: защита отнимается от наносимого урона, остаток падает в виде мяса
     *                   Fitness: эффективность расходования энергии в движение - линейная или кривая, чем кривее, тем меньше эволюционных очков это требует
     *                   
     *                   
     *             Carnivore + Herbivore + Offence + Deffence + Fitness  
     *             
     *             Минимальное значение - 0
     *             Максимальное значение - 4
     *             
     *             Всего у клетки 10 очков
     *             
     *             Поведение для нулевого значения:
     *             Carnivore: клетка не ест мяса, она на него даже не смотрит
     *             Herbovore: клетка не ест растительность
     *             Offence: урон 0, клетка не насит урон 
     *             Deffence: защита 0, клетка не имеет имунитета к атаке
     *             Fitness: расход энергии велик практически с минимальной скорости
     *             
     *             Поведение для макс значения:
     *             Carnivore: каждое очко добавляет 25% к усвоению пищи, при значении 4 пища усваивается полностью
     *             Herbovore: каждое очко добавляет 25% к усвоению пищи, при значении 4 пища усваивается полностью
     *             Offence: урон 4, клетка насит урон 4
     *             Deffence: защита 4, клетка отражает урон 4
     *             Fitness: расход энергии линейно растет со скоростью
     *             
     * 
     * Limits (Constants):
     *              Speed Limit: максимально возможная скорость движения 
     *                              нужна чтобы предотвратить экстраскорости на 
     *                              малыш размерах
     *              Deadly Size: критический вес, при достижении которого клетка умирает
     *            Division Size: размер при котором клетка делится пополам
     * 
     * Dynamic properties:
     *                    Speed: максимальная скорость вычисляется из текущего веса
     *                     Size: вес влияет на скорость
     *                     
     * Scores: фитнес функция время жизни * количество еды
     *                  Up Time: время жизни
     *               Eaten Size: сколько всего съедено
     * 
     * 
     * На вход нейронке подается:
     * Информация о ближайшием растении:
     * x, y, size
     * 
     * Информация о ближайшем мясе:
     * x, y, size
     * 
     * Информация о ближайшем сопернике
     * x, y, size, размер, защита, урон
     * 
     * информация о себе:
     * размер, атака, защита
     * 
     * 
     * Идеи
     *             Aggressiveness: влияет на принятие решения есть или драться 
     *                            решение принимает нейронка, ли бо же это должно в фитнес функции отражаться, 
     *                            меньше очков, если не соответвуешь темпераменту, тогда нужно на вход подавать это же значнеие
     *              подавать на вход нескокльо кусков еды, возможно видя бОльший размер, клетка устремится к нему
     *              подавать на вход несколько соперников, возможно видя варианты, клетка выберет менее опасного для нападения
     *              
     *              на карте зоны, полянки, где еды больше, но наносится статический урон
     *              
     *              что, если закольцевать выход нейронки на вход? петля обратной связи
     *              
     *              размер порции, которую может укусить за раз...... для начала сделать это пропорционально размеру, а потом скилом
     */

    
    public NeuralNetwork neuralNetwork;

    //Preset && Limits
    public const float birthSize = 1f;
    public const float deadSize = 0.1f;
    public const float divisionSize = 5f;
    public const float maxSpeedLimit = 10f;

    public const float birnSpeed = .25f;

    public const int maxSkillValue = 4;
    public const int totalAvailableSkillScores = 10;

    public const float skillMutationChance = 0.1f;
    public const int skillMutationMagnitude = 1;

    public const float nnMutationChance = .1f;
    public const float nnMutationMagnitude = .1f;

    public const float silentEnergyFine = 0f;

    //Skills
    //0 - carnivore;
    //1 - herbovore;
    //2 - damage;
    //3 - armor;
    public const int skillsNumber = 4;
    public int[] skills;
    public Color color = new Color(.5f, .5f, .5f);

    //Dynamic Properties
    public float size;

    //Scores
    public float upTime = 0f;
    public float totalEaten = 0f;
    public int divisionsNumber = 0;

    public int getCarnivore() {
        return skills[0];
    }

    public int getHerbovore()
    {
        return skills[1];
    }

    public int getDamage()
    {
        return skills[2];
    }

    public int getArmor()
    {
        return skills[3];
    }

    public float getScore() {
        return upTime + totalEaten + divisionsNumber;
    }

    public float getMaxSpeed() {
        return Mathf.Min(maxSpeedLimit, maxSpeedLimit * 1/size);
    }

    public Genom() {
        neuralNetwork = new NeuralNetwork(new int[2] { 3, 3 });
        size = birthSize;
        fillSkillScoresRandomly();
    }

    public Genom clone() {
        Genom newGenom = new Genom();
        newGenom.neuralNetwork = this.neuralNetwork.clone();
        newGenom.skills = (int[])skills.Clone();

        return newGenom;
    }

    public void crossWith(Genom g) {
        neuralNetwork.crossWith(g.neuralNetwork);
        for(int i = 0; i < skillsNumber; i++)
        {
            if(Random.value > .5)
            {
                skills[i] = g.skills[i];
            }
        }

        normalizeSkills();
    }

    public void mutate()
    {
        neuralNetwork.mutate(nnMutationMagnitude, nnMutationChance);

        for (int i = 0; i < skillsNumber; i++)
        {
            if (Random.value < skillMutationChance) {
                skills[i] += Random.Range(-skillMutationMagnitude, skillMutationMagnitude);
            }
        }

        normalizeSkills();
    }

    public void fillSkillScoresRandomly() {
        skills =  new int[skillsNumber];
        int totalAssignedScores = 0;
        while (totalAssignedScores < totalAvailableSkillScores) {
            int skillIndex = Random.Range(0, skillsNumber);
            if(skills[skillIndex] < maxSkillValue)
            {
                skills[skillIndex]++;
                totalAssignedScores++;
            }
        }

        calculateColor();
    }

    public void normalizeSkills()
    {
        int skillsScoresDelta = getSkillsScores() - totalAvailableSkillScores;
        while(skillsScoresDelta != 0)
        {
            int skillIndex = Random.Range(0, skillsNumber);
            if (skillsScoresDelta > 0)
            {
                if(skills[skillIndex] > 0)
                {
                    skills[skillIndex]--;
                    skillsScoresDelta--;
                }
            }
            else
            {
                if (skills[skillIndex] < maxSkillValue)
                {
                    skills[skillIndex]++;
                    skillsScoresDelta++;
                }
            }
        }

        calculateColor();
    }

    public int getSkillsScores() {
        int skillsScores = 0;
        foreach(int skillScore in skills)
        {
            skillsScores += skillScore;
        }

        return skillsScores;
    }

    public void calculateColor() {
        //0 - carnivore; --- RED 
        //1 - herbovore; --- GREEN
        //2 - damage;    --- RED
        //3 - armor;     --- BLUE
        //color.r = skills[0] * (1f / (2 * maxSkillValue)) + skills[1] * (1 / (2 * maxSkillValue));
        color.r = skills[0] * (1f / maxSkillValue);
        color.g = skills[1] * (1f / maxSkillValue);
        color.b = 0;// skills[3] * (1f / maxSkillValue);   
    }

    public void doStep(float force, float time)
    {
        float birned = Mathf.Abs(force) * time * birnSpeed + silentEnergyFine;
        size -= birned;
        upTime += time;
    }

    public bool isDied() {
        return size < deadSize;
    }

    public void eat(float energy, bool isHerbal = true) {
        float k = isHerbal ? (1/((float)getHerbovore())) : (1 / ((float)getCarnivore()));
        k = k > 0 ? k : 0.1f;
        energy *= k;

        totalEaten += energy;
        size += energy;
    }

    public bool isReadyToDivide()
    {
        return size > divisionSize;
    }

    public void divide()
    {
        size -= birthSize;
        divisionsNumber += 1;
    }

    public Vector3 getVectorToGoal(Vector3 vectorToNearFood, Vector3 vectorToNearCell) {
        if (getCarnivore() == 0 || vectorToNearCell.magnitude == 0 || getHerbovore() > getCarnivore())
        {
            return vectorToNearFood;
        }
        else if (getCarnivore() > getHerbovore() || vectorToNearFood.magnitude == 0)
        {
            return vectorToNearCell;
        }
        else if (getCarnivore() == getHerbovore()) {
            return vectorToNearFood.magnitude > vectorToNearCell.magnitude ? vectorToNearFood : vectorToNearCell;
        }
        else
        {
            return vectorToNearFood;
        }
    }
}
