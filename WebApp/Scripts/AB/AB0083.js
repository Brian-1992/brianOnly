
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var UrlReport = '/Report/A/AB0083A.aspx';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    var st_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0083/GetKindCombo',
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
            url: '/api/AB0083/GetWhmastCombo',
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
            xtype: 'combo',
            fieldLabel: '列印類型',
            name: 'P0',
            id: 'P0',
            store: st_kind,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            allowBlank: false,
            fieldCls: 'required',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            width: 280,
            listeners: {
                select: function (c, r, eo) {
                    Ext.getCmp('T1btn1').setDisabled(false);
                }
            }
        }, {
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
            fieldLabel: '病房別',
            name: 'P3',
            id: 'P3',
            store: st_whmast,
            queryMode: 'local',
            multiSelect: true,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                displayField: 'TEXT',
            valueField: 'VALUE'
        },{
            xtype: 'button',
            text: '列印',
            id: 'T1btn1',
            disabled: true,
            handler: function () {
                var p0 = T1Query.getForm().findField('P0').getValue() == null ? '' : T1Query.getForm().findField('P0').getValue();
                var p1 = T1Query.getForm().findField('P1').rawValue == null ? '' : T1Query.getForm().findField('P1').rawValue;
                var p2 = T1Query.getForm().findField('P2').rawValue == null ? '' : T1Query.getForm().findField('P2').rawValue;
                var p3 = T1Query.getForm().findField('P3').getValue() == null ? '' : T1Query.getForm().findField('P3').getValue();
                if (p0 == "") {
                    T1Query.getForm().findField('P0').focus();
                    Ext.Msg.alert('提醒', '列印類型必選');
                }
                else {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var qstring = '?p1=' + p1 + '&p2=' + p2 + '&p3=' + p3;
                    if (p0 == '01') {
                        UrlReport = '/Report/A/AB0083A.aspx';
                    }
                    else if (p0 == '02') {
                        UrlReport = '/Report/A/AB0083B.aspx';
                    }
                    else if (p0 == '03') {
                        UrlReport = '/Report/A/AB0083C.aspx';
                    }
                    else  {
                        UrlReport = '/Report/A/AB0083D.aspx';
                    }
                    Ext.getDom('mainContent').src = UrlReport + qstring;

                    $('iframe#mainContent').load(function () {
                        myMask.hide();
                    });
                }
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                Ext.getCmp('T1btn1').setDisabled(true);
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