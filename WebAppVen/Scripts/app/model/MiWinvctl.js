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
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]

});