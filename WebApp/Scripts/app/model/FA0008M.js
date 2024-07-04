Ext.define('WEBAPP.model.FA0008M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'ROWORDER', type: 'string' },   //排序用
        { name: 'MAT', type: 'string' },        //類別
        { name: 'PIQ_PA', type: 'string' },     //SUM(A.P_INV_QTY * A.PMN_AVGPRICE) AS ""期初存貨成本"",
        { name: 'IQ_IP', type: 'string' },      //SUM(A.IN_QTY * A.IN_PRICE) AS ""進貨成本"",
        { name: 'OQ_A_AP', type: 'string' },    //SUM((A.OUT_QTY - B.APLINQTY) * A.AVG_PRICE) AS ""(內湖)撥發成本"",
        { name: 'I_AP_P', type: 'string' },     //SUM(A.INVENTORYQTY * A.AVG_PRICE) AS ""盤盈"", --找出所有正值累加,
        { name: 'I_AP_M', type: 'string' },     //SUM(-(A.INVENTORYQTY * A.AVG_PRICE)) AS ""盤虧"", --找出所有負值累加,
        { name: 'A_PA', type: 'string' },       //SUM(B.APLINQTY * A.PMN_AVGPRICE) AS ""台北門診應收帳款"",
        { name: 'I_AP', type: 'string' },       //SUM(A.INV_QTY * A.AVG_PRICE) AS ""調整後期末存貨成本""
    ]
});