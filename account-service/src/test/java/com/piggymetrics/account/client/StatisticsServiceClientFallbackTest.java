package com.piggymetrics.account.client;

import com.piggymetrics.account.config.TestSecurityConfig;
import com.piggymetrics.account.domain.Account;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.system.CapturedOutput;
import org.springframework.boot.test.system.OutputCaptureExtension;
import org.springframework.context.annotation.Import;

import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(OutputCaptureExtension.class)
@SpringBootTest(properties = {
        "spring.cloud.openfeign.circuitbreaker.enabled=true"
})
@Import(TestSecurityConfig.class)
public class StatisticsServiceClientFallbackTest {

    @Autowired
    private StatisticsServiceClient statisticsServiceClient;

    @Test
    public void testUpdateStatisticsWithFailFallback(CapturedOutput output) {
        statisticsServiceClient.updateStatistics("test", new Account());

        assertThat(output.getOut() + output.getErr()).contains("Error during update statistics for account: test");
    }

}
