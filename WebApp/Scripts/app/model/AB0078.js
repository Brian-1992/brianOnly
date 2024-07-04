Ext.define('WEBAPP.model.AB0078', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DATA_YM', type: 'string' },
        { name: 'M_NHIKEY', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'DISC_UPRICE', type: 'string' },
        { name: 'AVG_PRICE', type: 'string' },
        { name: 'MN_INQTY', type: 'string' }, // 總進貨量
        { name: 'TOT_AMT', type: 'string' }, // MN_INQTY * AVG_PRICE 總進貨金額
        { name: 'INV_QTY', type: 'string' },
        { name: 'FNL_AMT', type: 'string' }, // INV_QTY * AVG_PRICE 結存金額
        { name: 'RR', type: 'string' }, // 期末比
        { name: 'CONT_PRICE', type: 'string' },
        { name: 'RATIO', type: 'string' }, // 優惠百分比
        { name: 'INSUAMOUNT', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'CONSUME_AMT', type: 'string' }, // 消耗金額
        { name: 'USE_QTY', type: 'string' }, // 消耗數量
    ]
});