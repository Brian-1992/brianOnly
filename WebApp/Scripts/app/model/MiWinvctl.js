Ext.define('WEBAPP.model.MiWinvctl', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },        // 自訂 UNIT_CODE + UI_CHANAME
        { name: 'SAFE_DAY', type: 'string' },
        { name: 'OPER_DAY', type: 'string' },
        { name: 'SHIP_DAY', type: 'string' },
        { name: 'SAFE_QTY', type: 'string' },
        { name: 'OPER_QTY', type: 'string' },
        { name: 'SHIP_QTY', type: 'string' },
        { name: 'DAVG_USEQTY', type: 'string' },
        { name: 'HIGH_QTY', type: 'string' },
        { name: 'LOW_QTY', type: 'string' },
        { name: 'MIN_ORDQTY', type: 'string' },
        { name: 'SUPPLY_WHNO', type: 'string' },
        { name: 'IS_AUTO', type: 'string' },
        { name: 'maxEXP_DATE', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'MMNAME', type: 'string' },
        { name: 'E_RESTRICTCODE', type: 'string' },
        { name: 'E_RESTRICTCODE_N', type: 'string' },
        { name: 'DANGERDRUGFLAG', type: 'string' },
        { name: 'DANGERDRUGFLAG_N', type: 'string' },
        { name: 'STATUS_DISPLAY', type: 'string' },
        { name: 'DAVG_USEQTY_90', type: 'string' },
        { name: 'SAFE_QTY_90', type: 'string' },
        { name: 'OPER_QTY_90', type: 'string' },
        { name: 'SHIP_QTY_90', type: 'string' },
        { name: 'HIGH_QTY_90', type: 'string' }
    ]

});