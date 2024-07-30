Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '../../../api/AA0156/SetBcItmanagers'; // 新增/修改/刪除
    var T1Name = "戰備換入換出報表";

    var reportUrl = '/Report/A/AA0156.aspx';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var MclassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0156/GetMclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '日期區間',
                    name: 'P0',
                    id: 'P0',
                    width: 150,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    width: 100,
                    labelWidth: 20,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P2',
                    id: 'P2',
                    width: 160,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    store: MclassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    anyMatch: true,
                    typeAhead: true,
                    forceSelection: true,
                    queryMode: 'local',
                    triggerAction: 'all',
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'fieldcontainer',
                    fieldLabel: '列印類別',
                    defaultType: 'radiofield',
                    labelWidth: 70,
                    defaults: {
                        flex: 1
                    },
                    layout: 'hbox',
                    items: [
                        {
                            boxLabel: '軍品轉入',
                            name: 'printType',
                            inputValue: '1',
                            id: 'radio1',
                            value: true
                        }, {
                            boxLabel: '軍品轉出',
                            name: 'printType',
                            inputValue: '2',
                            id: 'radio2'
                        }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢', handler: function () {

                        if (T1Query.getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '所有查詢條件為必填');
                            return;
                        }

                        if (Ext.getCmp('radio1').getValue() == true) {
                            T1Query.getForm().findField('P3').setValue(Ext.getCmp('radio1').inputValue);
                        } else {
                            T1Query.getForm().findField('P3').setValue(Ext.getCmp('radio2').inputValue);
                        }

                        reportAutoGenerate(true);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'hidden',
                    name: 'P3',
                    id: 'P3',
                    submitValue: true
                },
            ]
        }]
    });

    function reportAutoGenerate(autoGen) {
        if (autoGen) {
            var p0 = T1Query.getForm().findField('P0').rawValue;
            var p1 = T1Query.getForm().findField('P1').rawValue;
            var p2 = T1Query.getForm().findField('P2').getValue();
            var p3 = T1Query.getForm().findField('P3').getValue();
            var qstring = 'STARTDATE=' + p0 + '&ENDDATE=' + p1 + '&MAT_CLASS=' + p2 + '&PRINTTYPE=' + p3;
            Ext.getDom('mainContent').src = autoGen ? reportUrl + '?' + qstring : '';
        }
    };

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tabReport',
                region: 'north',
                frame: false,
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Query]
            },
            {
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

    reportAutoGenerate(false);

    T1Query.getForm().findField('P0').focus();
});
