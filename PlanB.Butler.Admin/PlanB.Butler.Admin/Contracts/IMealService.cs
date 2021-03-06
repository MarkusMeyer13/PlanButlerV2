﻿// Copyright (c) PlanB. GmbH. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using PlanB.Butler.Admin.Models;

namespace PlanB.Butler.Admin.Contracts
{
    /// <summary>
    /// IMealService.
    /// </summary>
    public interface IMealService
    {
        /// <summary>
        /// Gets the meals.
        /// </summary>
        /// <returns>Meals.</returns>
        Task<List<MealViewModel>> GetMeals();

        /// <summary>
        /// Creates the meal.
        /// </summary>
        /// <param name="meal">The meal.</param>
        /// <returns>Meal.</returns>
        Task<MealViewModel> CreateMeal(MealViewModel meal);

        /// <summary>
        /// Updates the meal.
        /// </summary>
        /// <param name="meal">The meal.</param>
        /// <returns>Meal.</returns>
        Task<MealViewModel> UpdateMeal(MealViewModel meal);

        /// <summary>
        /// Gets the meal.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Meal by Id.</returns>
        Task<MealViewModel> GetMeal(string id);

        /// <summary>
        /// Deletes the meal.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Succes or failure.</returns>
        Task<bool> DeleteMeal(string id);
    }
}
