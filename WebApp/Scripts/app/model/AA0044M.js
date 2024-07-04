
Ext.define('WEBAPP.model.AA0044M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'INV_QTY_N', type: 'string' },//結存
        { name: 'INV_QTY_L', type: 'string' },
        { name: 'APL_INQTY_N', type: 'string' },//進貨
        { name: 'APL_INQTY_L', type: 'string' },
        { name: 'APL_OUTQTY_N', type: 'string' },//撥發
        { name: 'APL_OUTQTY_L', type: 'string' },
        { name: 'TRN_INQTY_N', type: 'string' },//調撥入庫
        { name: 'TRN_INQTY_L', type: 'string' },
        { name: 'TRN_OUTQTY_N', type: 'string' },//調撥出庫
        { name: 'TRN_OUTQTY_L', type: 'string' },
        { name: 'ADJ_INQTY_N', type: 'string' },//調帳入庫
        { name: 'ADJ_INQTY_L', type: 'string' },
        { name: 'ADJ_OUTQTY_N', type: 'string' },//調帳出庫
        { name: 'ADJ_OUTQTY_L', type: 'string' },
        { name: 'BAK_INQTY_N', type: 'string' },//繳回入庫
        { name: 'BAK_INQTY_L', type: 'string' },
        { name: 'BAK_OUTQTY_N', type: 'string' },//繳回出庫
        { name: 'BAK_OUTQTY_L', type: 'string' },
        { name: 'EXG_INQTY_N', type: 'string' },//換貨入庫
        { name: 'EXG_INQTY_L', type: 'string' },
        { name: 'EXG_OUTQTY_N', type: 'string' },//換貨出庫
        { name: 'EXG_OUTQTY_L', type: 'string' },
        { name: 'MIL_INQTY_N', type: 'string' },//戰備入庫
        { name: 'MIL_INQTY_L', type: 'string' },
        { name: 'MIL_OUTQTY_N', type: 'string' },//戰備出庫
        { name: 'MIL_OUTQTY_L', type: 'string' },
        { name: 'ONWAY_QTY_N', type: 'string' },//在途入庫
        { name: 'ONWAY_QTY_L', type: 'string' },
        { name: 'REJ_OUTQTY_N', type: 'string' },//退貨出庫
        { name: 'REJ_OUTQTY_L', type: 'string' },
        { name: 'DIS_OUTQTY_N', type: 'string' },//報廢出庫
        { name: 'DIS_OUTQTY_L', type: 'string' },
        { name: 'INVENTORYQTY1_N', type: 'string' },//盤盈入庫
        { name: 'INVENTORYQTY1_L', type: 'string' },
        { name: 'INVENTORYQTY2_N', type: 'string' },//盤虧出庫
        { name: 'INVENTORYQTY2_L', type: 'string' },
        { name: 'USE_QTY_N', type: 'string' },//耗用量
        { name: 'USE_QTY_L', type: 'string' },
        { name: 'DATA_YM_N', type: 'string' },
        { name: 'DATA_YM_L', type: 'string' },
        { name: 'TOTQTY_N', type: 'string' },
        { name: 'TOTQTY_L', type: 'string' }

    ]
});