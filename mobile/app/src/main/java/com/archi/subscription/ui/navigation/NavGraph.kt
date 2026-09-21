package com.archi.subscription.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavType
import androidx.navigation.compose.*
import androidx.navigation.navArgument
import com.archi.subscription.ui.dashboard.DashboardScreen
import com.archi.subscription.ui.home.HomeScreen
import java.net.URLDecoder
import java.net.URLEncoder

@Composable
fun NavGraph() {
    val navController = rememberNavController()

    NavHost(navController = navController, startDestination = "home") {
        composable("home") {
            HomeScreen(
                onCustomerSelected = { customer ->
                    val name = URLEncoder.encode(customer.fullName, "UTF-8")
                    val email = URLEncoder.encode(customer.email, "UTF-8")
                    val phone = URLEncoder.encode(customer.phoneNumber, "UTF-8")
                    navController.navigate("dashboard/${customer.id}/$name/$email/$phone")
                }
            )
        }
        composable(
            route = "dashboard/{customerId}/{name}/{email}/{phone}",
            arguments = listOf(
                navArgument("customerId") { type = NavType.StringType },
                navArgument("name") { type = NavType.StringType },
                navArgument("email") { type = NavType.StringType },
                navArgument("phone") { type = NavType.StringType }
            )
        ) { entry ->
            DashboardScreen(
                customerId = entry.arguments?.getString("customerId") ?: "",
                customerName = URLDecoder.decode(entry.arguments?.getString("name") ?: "", "UTF-8"),
                customerEmail = URLDecoder.decode(entry.arguments?.getString("email") ?: "", "UTF-8"),
                customerPhone = URLDecoder.decode(entry.arguments?.getString("phone") ?: "", "UTF-8"),
                onBack = { navController.popBackStack() }
            )
        }
    }
}

