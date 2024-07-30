Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '../../../api/AA0086/SetBcItmanagers'; // 新增/修改/刪除
    var T1Name = "藥品醫材等值換入換出軍品明細表";

    var reportUrl = '/Report/A/AA0086.aspx';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


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
                    fieldLabel: '查詢日期',
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
                    labelWidth:20,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'fieldcontainer',
                    fieldLabel: '列印類別',
                    defaultType: 'radiofield',
                    labelWidth:70,
                    defaults: {
                        flex: 1
                    },
                    layout: 'hbox',
                    items: [
                        {
                            boxLabel: '軍品轉出',
                            name: 'printType',
                            inputValue: 'frwh',
                            id: 'radio1',
                            value: true
                        }, {
                            boxLabel: '軍品轉入',
                            name: 'printType',
                            inputValue: 'towh',
                            id: 'radio2'
                        }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢', handler: function () {

                        if (T1Query.getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>查詢日期</span>為必填');
                            return;
                        }
                        
                        if (Ext.getCmp('radio1').getValue() == true) {
                            T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio1').inputValue);
                        } else {
                            T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio2').inputValue);
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
                    name: 'P2',
                    id: 'P2',
                    submitValue: true
                },
            ]
        }]
    });

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        //resizeTabs: true,
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BcItmanager', { // 定義於/Scripts/app/store/BcItmanager.js 
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records) {
                
                if (records.length > 0) {
                    T1Grid.down('#update').setDisabled(false);
                    T1Grid.down('#cancel').setDisabled(false);
                } else {
                    T1Grid.down('#update').setDisabled(true);
                    T1Grid.down('#cancel').setDisabled(true);
                }
            }
        },

    });
    function T1Load(clearMsg) {
        T1Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1print', text: '列印', handler: function () {
                    if (Ext.getCmp('radio1').getValue() == true) {
                        T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio1').inputValue);
                    } else {
                        T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio2').inputValue);
                    }

                    showReport();
                }
            }]
    });

    //function showReport() {
    //    if (!win) {
    //        var winform = Ext.create('Ext.form.Panel', {
    //            id: 'iframeReport',
    //            //height: '100%',
    //            //width: '100%',
    //            layout: 'fit',
    //            closable: false,
    //            html: '<iframe src="' + reportUrl + '?STARTDATE=' + getDateString(T1Query.getForm().findField('P0').getValue())
    //            + '&ENDDATE=' + getDateString(T1Query.getForm().findField('P1').getValue())
    //            + '&PRINTTYPE=' + T1Query.getForm().findField('P2').getValue()
    //            + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
    //            buttons: [{
    //                text: '關閉',
    //                handler: function () {
    //                    this.up('window').destroy();
    //                }
    //            }]
    //        });
    //        var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
    //    }
    //    win.show();
    //}

    function reportAutoGenerate(autoGen) {
        if (autoGen) {
            var p0 = getDateString(T1Query.getForm().findField('P0').getValue());   //動態年月
            var p1 = getDateString(T1Query.getForm().findField('P1').getValue()); //車型代碼
            var p2 = T1Query.getForm().findField('P2').getValue();
            var p3 = session['UserId'];
            var qstring = 'STARTDATE=' + p0 + '&ENDDATE=' + p1 + '&PRINTTYPE=' + p2 + '&PRINTUSER=' + p3;
            Ext.getDom('mainContent').src = autoGen ? reportUrl + '?' + qstring : '';
        }
    };

    function getDateString(date) {
        var y = date.getFullYear();
        var m = (date.getMonth() + 1).toString();
        var d = (date.getDate()).toString();
        var mm = m.length > 1 ? m : "0" + m;
        var dd = d.length > 1 ? d : "0" + d;
        return y + "-" + mm + "-" + dd;
    }

    
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
