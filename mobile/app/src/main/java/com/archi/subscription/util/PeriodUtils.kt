package com.archi.subscription.util

import java.time.ZonedDateTime
import java.time.ZoneOffset
import java.time.format.DateTimeFormatter

object PeriodUtils {
    /** Parse an ISO date string and return a UTC ZonedDateTime */
    fun parseDate(iso: String): ZonedDateTime =
        ZonedDateTime.parse(iso.replace(" ", "T").let { if (it.endsWith("Z") || it.contains("+")) it else "${it}Z" })

    /** Compute period string "yyyy-MM" from a date with a month offset */
    fun periodFromDate(date: ZonedDateTime, offsetMonths: Long): String {
        val d = date.withZoneSameInstant(ZoneOffset.UTC).plusMonths(offsetMonths).withDayOfMonth(1)
        return d.format(DateTimeFormatter.ofPattern("yyyy-MM"))
    }

    /** Get "yyyy-MM" from a UTC date */
    fun currentPeriod(date: ZonedDateTime): String =
        date.withZoneSameInstant(ZoneOffset.UTC).format(DateTimeFormatter.ofPattern("yyyy-MM"))

    /** Format an ISO date for display */
    fun formatDate(iso: String): String = try {
        val d = parseDate(iso)
        d.format(DateTimeFormatter.ofPattern("EEE, d MMM yyyy"))
    } catch (_: Exception) { iso }

    /** Format date short */
    fun formatShort(iso: String): String = try {
        val d = parseDate(iso)
        d.format(DateTimeFormatter.ofPattern("d MMM yyyy"))
    } catch (_: Exception) { iso }
}

