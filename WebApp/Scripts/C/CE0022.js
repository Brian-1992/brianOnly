Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;
    var gloval = null; //CHK_NO
    var glUPDN_STATUS = null; //UPDN_STATUS
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T3GetExcel = '../../../api/CE0022/Excel';
    var T3LastRec = null;

    var todayDateString = '';
    function getTodayDate() {
        Ext.Ajax.request({
            url: '/api/CE0002/CurrentDate',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    todayDateString = data.msg;
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getTodayDate();
    function isEditable(chk_ym) {
        
        if (chk_ym.substring(0, 5) != todayDateString) {
            return false;
        } else {
            return true;
        }
    }

    //#region 主畫面

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'CHK_WH_NO',
        fieldLabel: '庫房代碼',
        storeAutoLoad: true,
        labelWidth: 60,
        width: 180,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑，根據規格書參考CE0014
        queryUrl: '/api/CE0014/GetWhnoCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            },
            focus: function (field, event, eOpts) {
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });
    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        itemId: 'queryform1',
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
        items: [
            //{
            //   xtype: 'panel',
            //   id: 'PanelP1',
            //   border: false,
            //   layout: 'hbox',
            //   items: [
            //       {
            //           xtype: 'button',
            //           text: '手機板',
            //           handler: function () {
            //               location.href = "../../../Form/Index/CE0003";
            //           }
            //       }]
            //   },
            {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    wh_NoCombo,
                    {
                        xtype: 'monthfield',
                        fieldLabel: '盤點年月',
                        name: 'D0',
                        id: 'D0',
                        labelWidth: 60,
                        width: 160,
                        value: getDefaultValue()
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤點人員',
                        name: 'peop',
                        id: 'peop',
                        labelWidth: 60,
                        width: 180,
                        value: session['UserName']
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: T1Load
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            msglabel('訊息區:');
                        }
                    }
                ]
            }]
    });
    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;
        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;
        var mm = m >= 10 ? m.toString() : "0" + m;
        //var mm = m >= 10 ? m.toString() : "0" + m.toString();
        return yyyy.toString() + mm;
    }

    var T1Store = Ext.create('WEBAPP.store.CE0022', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件WH_NO的值代入參數
                var np = {
                    wh_no: T1Query.getForm().findField('CHK_WH_NO').getValue(),
                    d0: T1Query.getForm().findField('D0').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        if (T1Query.getForm().findField('CHK_WH_NO').getValue() == null && T1Query.getForm().findField('D0').getValue() == null) {
            Ext.Msg.alert('提醒', '庫房號碼、盤點年月需要二擇一才能查詢!');
        }
        else {
            T1Tool.moveFirst();
        }
        msglabel('訊息區:');
    }

    // toolbar 
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
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
        // grid columns
        columns: {
            items: [{
                xtype: 'rownumberer',
                width: 30
            }, {
                text: "庫房代碼",
                dataIndex: 'CHK_WH_NO',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                text: "盤點日期",
                dataIndex: 'CHK_YM',
                style: 'text-align:left',
                align: 'left',
                width: 60,
            }, {
                text: "庫別級別",
                dataIndex: 'CHK_WH_GRADE',
                style: 'text-align:left',
                align: 'left',
                width: 60,
            }, {
                text: "庫別分類",
                dataIndex: 'CHK_WH_KIND',
                style: 'text-align:left',
                align: 'left',
                width: 60
            }, {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD',
                style: 'text-align:left',
                align: 'right',
                width: 60,

            }, {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                style: 'text-align:left',
                align: 'right',
                width: 80,
            }, {
                text: "盤點數量/盤點總量",
                dataIndex: 'MERGE_NUM_TOTAL',
                style: 'text-align:left',
                align: 'right',
                width: 135,
            }, {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                style: 'text-align:left',
                align: 'left',
                width: 120,
                renderer: function (val, meta, record) {
                    if (val != null) {
                        return '<a href=javascript:void(0)>' + val + '</a>';
                    }
                }
            }, {
                text: "負責人員",
                dataIndex: 'CHK_KEEPER',
                style: 'text-align:left',
                align: 'left',
                width: 80,
            }, {
                text: "狀態",
                dataIndex: 'CHK_STATUS',
                style: 'text-align:left',
                align: 'left',
                width: 60,
            }, {
                header: "",
                flex: 1
            }]
        },
        viewConfig: {
            listeners: {
                selectionchange: function (model, records) {
                    T1LastRec = records[0];
                },
                cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {
                    T1LastRec = record;

                    if (cellIndex != 9) {
                        return;
                    }
                    var clickedDataIndex = view.panel.headerCt.getHeaderAtIndex(cellIndex).dataIndex;
                    var clickedColumnName = view.panel.headerCt.getHeaderAtIndex(cellIndex).text;
                    var clickedCellValue = record.get(clickedDataIndex); //得到值 clickedCellValue, CHK_NO

                    gloval = clickedCellValue;

                    T3LastRec = record;

                    g2Window.setTitle("複盤盤數量輸入作業: " + T3LastRec.data.CHK_WH_NO + " " + T3LastRec.data.CHK_YM + " " + T3LastRec.data.CHK_PERIOD + " " + T3LastRec.data.CHK_TYPE +
                        " " + T3LastRec.data.CHK_STATUS + " " + T3LastRec.data.CHK_WH_KIND + " " + T3LastRec.data.CHK_NO + " " + session['UserName']);

                    setG2WindowButtons(Number(T3LastRec.data.UPDN_STATUS));
                    T3Load();
                    g2Window.setY(0);
                    g2Window.show();


                }
            }
        }
    });







    //#endregion

    //#region G2Window

    var viewModel = Ext.create('WEBAPP.store.CE0022VM');

    var T3Store = viewModel.getStore('allini');
    function T3Load() {
        T3Store.getProxy().setExtraParam("chk_no", T3LastRec.data.CHK_NO);
        T3Tool.moveFirst();
    }

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'downloadExcel', text: '下載盤點單', disabled: false, handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: T3LastRec.data.CHK_NO + '-' + session['UserName'] + '.xls' });
                    p.push({ name: 'chk_no', value: T3LastRec.data.CHK_NO });
                    PostForm(T3GetExcel, p);

                }
            },
            {
                xtype: 'filefield',
                name: 'uploadExcel',
                id: 'uploadExcel',
                buttonText: '上傳盤點單',
                buttonOnly: true,
                padding: '0 4 0 0',
                width: 72,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        var file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            Ext.getCmp('import').fileInputEl.dom.value = '';
                            msglabel('');
                        } else {
                            msglabel("已選擇檔案");
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/CE0022/Upload",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask.hide();
                                    if (!data.success) {
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                                    }
                                    else {
                                        msglabel("訊息區:資料匯入成功");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';

                                        T3Load();
                                        T1Load();
                                    }
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('uploadExcel').setRawValue("");
                                }
                            });
                        }
                    }

                }

            },
            {
                id: 'saveG2',
                text: "確認儲存",
                disabled: false,
                handler: function () {
                    var tempData = T3Grid.getStore().data.items;
                    var data = [];
                    let CHK_QTY = '';
                    let CHK_REMARK = '';
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }

                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();

                    Ext.Ajax.request({
                        url: '/api/CE0022/Save',
                        method: reqVal_p,
                        contentType: "application/json",
                        params: { ITEM_STRING: Ext.util.JSON.encode(data), CHK_NO: T3LastRec.data.CHK_NO },
                        success: function (response) {
                            myMask.hide();
                            var data = Ext.decode(response.responseText);
                            if (data.success) {

                                //msglabel('訊息區:資料更新成功');
                                T3Store.load({
                                    params: {
                                        start: 0
                                    }
                                });

                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        },

                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            },
            {
                id: 'FinalProID',
                text: "完成盤點",
                disabled: false,
                handler: function () {
                    setFinalData();
                }
            }
        ]
    });

    //完成盤點
    function setFinalData() {

        //先儲存
        var tempData = T3Grid.getStore().data.items;
        var data = [];
        let CHK_QTY = '';
        let CHK_REMARK = '';
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].dirty) {
                data.push(tempData[i].data);
            }
        }

        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0022/Save',
            method: reqVal_p,
            contentType: "application/json",
            params: { ITEM_STRING: Ext.util.JSON.encode(data), CHK_NO: T3LastRec.data.CHK_NO },
            success: function (response) {
                
                myMask.hide();

                Ext.Ajax.request({
                    url: '/api/CE0022/FinalPro',
                    method: reqVal_p,
                    params: { CHK_NO: T3LastRec.data.CHK_NO },
                    success: function (response) {
                        var data = Ext.decode(response.responseText);

                        if (data.success) {
                            var tb_msg = data.msg;
                            var tb_etts = data.etts;
                            var tips = "";
                            var data = Ext.decode(response.responseText);
                            if (data.success) {

                                //msglabel('訊息區:資料更新成功');

                                if (tb_msg == "尚有負責的品項未盤點!") {  //尚有負責的品項未盤點
                                    for (var i = 0; i < tb_etts.length; i++) {
                                        tips = tips + "院內碼: " + tb_etts[i].MMCODE + ", 藥槽號: " + tb_etts[i].STORE_LOC + "<br>";
                                    }
                                    tips = tips + "品項未盤點";

                                    Ext.Msg.alert('提醒', tips);
                                } else if (tb_msg == "您已經完成此單號的盤點!") {   //做update
                                    T1Load();                                      //更新主畫面
                                    T3Load();                                       //更新副畫面(第二層)
                                    T3LastRec.data.UPDN_STATUS = '4';
                                    setG2WindowButtons(T3LastRec.data.UPDN_STATUS);
                                    msglabel('訊息區:' + tb_msg);
                                }

                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }

                        }
                    },
                    failure: function (response, options) {

                    }
                });
            },
            failure: function (response, action) {
                myMask.hide();
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }


    function setG2WindowButtons(updn_status) {
        if (isEditable(T1LastRec.data.CHK_YM) == false) {
            Ext.getCmp('downloadExcel').disable();
            Ext.getCmp('uploadExcel').disable();
            Ext.getCmp('saveG2').disable();
            Ext.getCmp('FinalProID').disable();
            return;
        }
        switch (Number(updn_status)) {

            case 4:
                Ext.getCmp('downloadExcel').disable();
                Ext.getCmp('uploadExcel').disable();
                Ext.getCmp('saveG2').disable();
                Ext.getCmp('FinalProID').disable();
                break;
            default:
                Ext.getCmp('downloadExcel').enable();
                Ext.getCmp('uploadExcel').enable();
                Ext.getCmp('saveG2').enable();
                Ext.getCmp('FinalProID').enable();
        }
    }

    var cellEditingPlugin = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1
    })

    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        id: 't3Grid',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }],

        columns: [{
            xtype: 'rownumberer',
            width:40
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 120
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80
        }, {
            text: "盤點量",
            style: 'text-align:left; color:red',
            align: 'right',
            dataIndex: 'CHK_QTY',
            width: 70,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                listeners: {
                    change: function (field, newVal, oldVal) {
                    },
                }
            },
        }, {
            text: "盤點人員",
            dataIndex: 'CHK_UID_NAME',
            width: 120
        },  {
            text: "預計盤點日期",
            dataIndex: 'CHK_PRE_DATE',
            width: 100
        },{
            text: "盤點時間",
            dataIndex: 'CHK_TIME',
            width: 200,
            renderer: function (value, meta, record) {
                if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (Taipei Standard Time)") {  //value值是 null時候
                    return '';
                } else if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (台北標準時間)") {
                    return '';
                } else {
                    return Ext.util.Format.date(value, 'Xmd H:i:s');
                }
            }
        }, {
            dataIndex: 'UPDN_STATUS',
            width: '10%',
            hidden: true
        },
        {
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            beforeedit: function (editor, e) {
                if (isEditable(T1LastRec.data.CHK_YM) == false) {
                    return false;
                }
                // UPDN_STATUS是4 則不可填寫
                if (e.colIdx === 5 && e.record.get('UPDN_STATUS') == '4') {
                    return false;
                }
            }
        },
        viewConfig: {
            listeners: {
                itemkeydown: function (grid, rec, item, idx, e) {
                    if (e.keyCode == 38) { //上
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx - 1, editPlugin.context.colIdx);
                    } else if (e.keyCode == 40) { //下
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx + 1, editPlugin.context.colIdx);
                    }

                    //if (e.keyCode == 13) { //enter
                    //    T3Load();
                    //}

                    //alert('The press key is' + e.getKey());
                }
            }
        },
    });

    function timeFormat(upload_date) {
        if (upload_date == null || upload_date == undefined) {
            return '';
        }

        var temp = new Date(upload_date);
        var yyy = (temp.getFullYear() - 1911).toString();
        var m = temp.getMonth() + 1;
        var d = temp.getDate();
        var mm = m < 10 ? "0" + m.toString() : m.toString();
        var dd = d < 10 ? "0" + d.toString() : d.toString();

        return '<span>' + yyy + mm + dd + '</span>';
    }

    var g2Window = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'g2Window',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't3Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T3Grid]
        }],
        buttons: [
            {
                id: 'closeG2Window',
                disabled: false,
                text: '關閉',
                handler: function () {
                    g2Window.hide();
                }
            }]
    });
    //#endregion


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 'T1Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T1Grid]
        }]
    });

    Ext.on('resize', function () {
        windowHeight = $(window).height();
        windowWidth = $(window).width();
        g2Window.setHeight(windowHeight);
    });

    var myMask = new Ext.LoadMask(Ext.getCmp('t3Grid'), { msg: '處理中...' });

    T1Load(); // 進入畫面時自動載入一次資料
});