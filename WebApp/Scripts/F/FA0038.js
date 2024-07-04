
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var UrlReport = '/Report/F/FA0038A.aspx';
    var T1GetExcel = '../../../api/FA0038/ExcelA';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    var st_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0038/GetKindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;
        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;
        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        return yyyy.toString() + mm;
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
                    Ext.getCmp('T1btn2').setDisabled(false);
                }
            }
        }, {
            xtype: 'monthfield',
            fieldLabel: '開始年月',
            name: 'P1',
            id: 'P1',
            value: getDefaultValue(),
            width: 160
        }, {
            xtype: 'monthfield',
            fieldLabel: '至',
            name: 'P2',
            id: 'P2',
            labelWidth: 7,
            value: getDefaultValue(),
            width: 120
        }, {
            xtype: 'button',
            text: '列印',
            id: 'T1btn1',
            disabled: true,
            handler: function () {
                var p0 = T1Query.getForm().findField('P0').getValue() == null ? '' : T1Query.getForm().findField('P0').getValue();
                var p1 = T1Query.getForm().findField('P1').rawValue == null ? '' : T1Query.getForm().findField('P1').rawValue;
                var p2 = T1Query.getForm().findField('P2').rawValue == null ? '' : T1Query.getForm().findField('P2').rawValue;
                if (p0 == "") {
                    T1Query.getForm().findField('P0').focus();
                    Ext.Msg.alert('提醒', '列印類型必選');
                }
                else {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var qstring = '?p1=' + p1 + '&p2=' + p2 ;
                    if (p0 == '01') {
                        UrlReport = '/Report/F/FA0038A.aspx';
                    }
                    else if (p0 == '02') {
                        UrlReport = '/Report/F/FA0038B.aspx';
                    }
                    else if (p0 == '03') {
                        UrlReport = '/Report/F/FA0038C.aspx';
                    }
                    else {
                        UrlReport = '/Report/F/FA0038D.aspx';
                    }
                    Ext.getDom('mainContent').src = UrlReport + qstring;

                    $('iframe#mainContent').load(function () {
                        myMask.hide();
                    });
                }
            }
            }, {
                xtype: 'button',
                text: '匯出',
                id: 'T1btn2',
                disabled: true,
                handler: function () {
                    var p0 = T1Query.getForm().findField('P0').getValue() == null ? '' : T1Query.getForm().findField('P0').getValue();
                    var p1 = T1Query.getForm().findField('P1').rawValue == null ? '' : T1Query.getForm().findField('P1').rawValue;
                    var p2 = T1Query.getForm().findField('P2').rawValue == null ? '' : T1Query.getForm().findField('P2').rawValue;
                    if (p0 == "") {
                        T1Query.getForm().findField('P0').focus();
                        Ext.Msg.alert('提醒', '列印類型必選');
                    }
                    else {
                        var fn = '';
                        if (p0 == '01') {
                            T1GetExcel = '../../../api/FA0038/ExcelA';
                            fn = '三級庫以上進貨資料.xls';
                        }
                        else if (p0 == '02') {
                            T1GetExcel = '../../../api/FA0038/ExcelB';
                            fn = '二級庫以上調帳資料.xls';
                        }
                        else if (p0 == '03') {
                            T1GetExcel = '../../../api/FA0038/ExcelC';
                            fn = '藥庫進貨資料.xls';
                        }
                        else {
                            T1GetExcel = '../../../api/FA0038/ExcelD';
                            fn = '累計進貨金額.xls';
                        }
                        var p = new Array();
                        p.push({ name: 'FN', value: fn });
                        p.push({ name: 'P1', value: p1 });
                        p.push({ name: 'P2', value: p2 });
                        PostForm(T1GetExcel, p);
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
                Ext.getCmp('T1btn2').setDisabled(true);
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