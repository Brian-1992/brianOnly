
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var UrlReport = '/Report/F/FA0053.aspx';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0053/GetKindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_whmast = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0053/GetWhmastCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    function getFirstday() {
        var date = new Date(), y = date.getFullYear(), m = date.getMonth();
        var firstDay = new Date(y, m, 1);
        var lastDay = new Date(y, m + 1, 0);
        return firstDay
    }

    var v_mat = '';
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P4',
        name: 'P4',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 160,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0053/GetMmCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: v_mat
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,

        items: [{
            xtype: 'datefield',
            fieldLabel: '日期範圍',
            name: 'P1',
            id: 'P1',
            vtype: 'dateRange',
            dateRange: { end: 'P2' },
            padding: '0 4 0 4',
            width: 160,
            value: getFirstday()
        }, {
            xtype: 'datefield',
            fieldLabel: '至',
            labelWidth: 10,
            name: 'P2',
            id: 'P2',
            labelSeparator: '',
            vtype: 'dateRange',
            dateRange: { begin: 'P1' },
            padding: '0 4 0 4',
            width: 90,
            value: getToday()
        }, {
            xtype: 'combo',
            fieldLabel: '庫別',
            name: 'P3',
            id: 'P3',
            store: st_whmast,
            queryMode: 'local',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            displayField: 'TEXT',
            valueField: 'VALUE',
            listeners: {
                select: function (combo, record, index) {
                    if (record.data.EXTRA1 != "") {
                        v_mat = record.data.EXTRA1;
                    }
                }
            }
        },
            T1QuryMMCode,
        {
            xtype: 'combo',
            fieldLabel: '診別',
            name: 'P5',
            id: 'P5',
            store: st_kind,
            queryMode: 'local',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
            displayField: 'TEXT',
            valueField: 'VALUE'
        },
        {
            xtype: 'button',
            text: '列印',
            id: 'T1btn1',
            //disabled: true,
            handler: function () {
                var p1 = T1Query.getForm().findField('P1').rawValue == null ? '' : T1Query.getForm().findField('P1').rawValue;
                var p2 = T1Query.getForm().findField('P2').rawValue == null ? '' : T1Query.getForm().findField('P2').rawValue;
                var p3 = T1Query.getForm().findField('P3').getValue() == null ? '' : T1Query.getForm().findField('P3').getValue();
                var p4 = T1Query.getForm().findField('P4').getValue() == null ? '' : T1Query.getForm().findField('P4').getValue();
                var p5 = T1Query.getForm().findField('P5').getValue() == null ? '' : T1Query.getForm().findField('P5').getValue();
                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                myMask.show();
                var qstring = '?p1=' + p1 + '&p2=' + p2 + '&p3=' + p3 + '&p4=' + p4 + '&p5=' + p5 ;

                Ext.getDom('mainContent').src = UrlReport + qstring;

                $('iframe#mainContent').load(function () {
                    myMask.hide();
                });

            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                //Ext.getCmp('T1btn1').setDisabled(true);
                Ext.getDom('mainContent').src = '';
            }
        }
        ]
    });

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        resizeTabs: true,
        layout: 'fit',
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }
        ]

    });


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 'tabReport',
            region: 'north',
            frame: false,
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [form1]
        }, {
            region: 'center',
            xtype: 'form',
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
        }
        ]
    });

});