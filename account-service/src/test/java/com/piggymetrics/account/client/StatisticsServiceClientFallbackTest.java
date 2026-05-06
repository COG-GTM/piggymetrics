package com.piggymetrics.account.client;

import com.piggymetrics.account.domain.Account;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.springframework.boot.test.rule.OutputCapture;

import static org.hamcrest.Matchers.containsString;

/**
 * @author cdov
 */
public class StatisticsServiceClientFallbackTest {

    private StatisticsServiceClient statisticsServiceClient;

    @Rule
    public final OutputCapture outputCapture = new OutputCapture();

    @Before
    public void setup() {
        statisticsServiceClient = new StatisticsServiceClientFallback();
        outputCapture.reset();
    }

    @Test
    public void testUpdateStatisticsWithFailFallback() {
        statisticsServiceClient.updateStatistics("test", new Account());

        outputCapture.expect(containsString("Error during update statistics for account: test"));
    }

}
