Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var DataTime='';

var P0  ='';
var P1  ='';
var P2  ='';
var P3  ='';
var P4  ='';
var P5 = ''; 




Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });



    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '廠商碼',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 100, // 可輸入最大長度為100
                    padding: '0 4 4 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '瓶號',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 4 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 8 4'
                }

            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '更換日期',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: mLabelWidth,
                    width: 145
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P4',
                    id: 'P4',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 15,
                    width: 100,
                }, {
                    xtype: 'radiogroup',
                    width: 150,
                    padding: '4 4 0 4',
                    name: 'QQ',
                    items: [
                        { boxLabel: '現況', name: 'P5', id: 'aa', inputValue: 'N', width: 70, checked: true },
                        { boxLabel: '歷史資料', name: 'P5', id: 'bb', inputValue: 'H' }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            if (f.findField('P4').rawValue < f.findField('P3').rawValue) {
                                Ext.MessageBox.alert('提示', '更換日期(起) 不可以大於更換日期(迄)');
                            }
                            else {
                                 P0 = T1Query.getForm().findField('P0').getValue();
                                 P1 = T1Query.getForm().findField('P1').getValue();
                                 P2 = T1Query.getForm().findField('P2').getValue();
                                 P3 = T1Query.getForm().findField('P3').rawValue;
                                 P4 = T1Query.getForm().findField('P4').rawValue;
                                 P5 = T1Query.getForm().findField('P5').getValue();

                                T1Load();
                            }
                        }
                        else {
                            Ext.MessageBox.alert('提示', '輸入資料格式錯誤');
                        }

                    },
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('label1').setValue('<span style=color:red>資料日期:' + DataTime + '(說明:異動資料非即時轉入三總，所以可能非最新資料)</span>');
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }]
            }, {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [{
                    id: 'label1',
                    name: 'label1',
                    xtype: 'displayfield',
                    //value: '<span style=color:red>資料日期:' + DataTime + '(說明:異動資料非即時轉入三總，所以可能非最新資料)</span>',
                    padding: '0 4 0 4',
                    width: 450
                }]
            }]
    });

    var T1Store = Ext.create('WEBAPP.store.PH_AIRST', { // 定義於/Scripts/app/store/PH_AIRST.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0:  P0 ,
                    p1:  P1 ,
                    p2:  P2 ,
                    p3:  P3 ,
                    p4:  P4 ,
                    p5: P5
                };
                Ext.apply(store.proxy.extraParams, np);

                T1Grid.columns[2].setVisible(!T1Query.getForm().findField('P5').getValue());

            },
        }
    });



    function T1Load() {
        T1Tool.moveFirst();
    }
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            itemId: 'export', text: '匯出', disabled: false,
            handler: function () {
                Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                    if (btn === 'yes') {
                        var p = new Array();
                        p.push({ name: 'p0', value: P0 }); //SQL篩選條件
                        p.push({ name: 'p1', value: P1 }); //SQL篩選條件
                        p.push({ name: 'p2', value: P2 }); //SQL篩選條件
                        p.push({ name: 'p3', value: P3 }); //SQL篩選條件
                        p.push({ name: 'p4', value: P4 }); //SQL篩選條件
                        p.push({ name: 'p5', value: P5 }); //SQL篩選條件
                        PostForm('/api/BH0005/Excel', p);

                    }
                });
            }
        }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                xtype: 'datecolumn',
                text: "更換日期",
                dataIndex: 'TXTDAY',
                width: 70,
                format: 'Xmd'
            }, {
                text: "更換類別",
                dataIndex: 'EXTYPE',
                width: 70,
                renderer: function (value) {
                    var str = '';
                    switch (value) {
                        case 'GO':
                            str = '取走';
                            break;
                        case 'GI':
                            str = '換入';
                            break;
                        case 'CH':
                            str = '修改';
                            break;
                        default: str = '';
                            break;
                    }
                    return str;
                }
            }, {
                text: "廠商碼",
                dataIndex: 'AGEN_NO',
                width: 80
            }, {
                text: "品名",
                dataIndex: 'NAMEC',
                width: 150
            }, {
                text: "瓶號",
                dataIndex: 'FBNO',
                width: 100
            }, {
                text: "尺寸",
                dataIndex: 'XSIZE',
                width: 70
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }
        ]
    });

    var d = new Date();
    m = d.getMonth(); //current month
    y = d.getFullYear(); //current year

    T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P3').setValue(new Date(y, m, 1));
    T1Query.getForm().findField('P4').setValue(d);


    function setDataTime() {
        Ext.Ajax.request({
            url: '../../../api/BH0005/GetDataTime',
            method: reqVal_g,
            success: function (response) {
                DataTime = Ext.decode(response.responseText);
                T1Query.getForm().findField('label1').setValue('<span style=color:red>資料日期:' + DataTime + '(說明:異動資料非即時轉入三總，所以可能非最新資料)</span>');
            },
            failure: function (response, options) {

            }
        });
    }
    setDataTime();

});
